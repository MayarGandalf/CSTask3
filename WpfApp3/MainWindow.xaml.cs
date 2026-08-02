using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3
{
    /// <summary>
    /// Основное окно приложения. Содержит логику загрузки, фильтрации, пагинации и экспорта данных.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PersonManager _repository;
        private readonly PaginationService _pagination;
        private readonly FilterService _filterService;
        private readonly ExportService _exportService;
        private readonly ImportService _importService;
        private readonly ExcelExporter _excelExporter;
        private readonly XmlExporter _xmlExporter;

        private ObservableCollection<Person> _persons = new();
        private FilterCriteria _currentFilter = new FilterCriteria();

        /// <summary>
        /// Конструктор главного окна. Инициализирует сервисы и загружает первую страницу данных.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DgRecords.ItemsSource = _persons;

            _repository = new PersonManager();
            _pagination = new PaginationService(100);
            _filterService = new FilterService();
            _excelExporter = new ExcelExporter();
            _xmlExporter = new XmlExporter();
            _exportService = new ExportService(_repository);
            _importService = new ImportService(new Importer(), _repository);

            LoadPage();
        }

        /// <summary>
        /// Загружает текущую страницу данных с применёнными фильтрами.
        /// Выполняет GetPaged и GetTotalCount параллельно в фоновых потоках.
        /// </summary>
        private async void LoadPage()
        {
            try
            {
                var dataTask = Task.Run(() => _repository.GetPaged(_pagination.Skip, _pagination.PageSize, _currentFilter));
                var totalTask = Task.Run(() => _repository.GetTotalCount(_currentFilter));

                await Task.WhenAll(dataTask, totalTask);

                var data = await dataTask;
                var total = await totalTask;

                _pagination.SetTotalRecords(total);

                _persons.Clear();
                foreach (var p in data)
                    _persons.Add(p);

                UpdateUI();
            }
            catch (Exception exception)
            {
                TbStatus.Text = $"❌ Ошибка загрузки: {exception.Message}";
            }
        }

        /// <summary>
        /// Обновляет элементы управления пагинацией и статусную строку.
        /// </summary>
        private void UpdateUI()
        {
            TbPageNumber.Text = _pagination.CurrentPage.ToString();
            TbTotalPages.Text = _pagination.TotalPages.ToString();
            TbTotalRecords.Text = _pagination.TotalRecords.ToString();
            TbStatus.Text = $"Показано: {_persons.Count} (всего: {_pagination.TotalRecords})";

            BtnPrevPage.IsEnabled = _pagination.CanGoPrevious;
            BtnNextPage.IsEnabled = _pagination.CanGoNext;
        }

        /// <summary>
        /// Применяет текущие фильтры из полей ввода и перезагружает данные с первой страницы.
        /// </summary>
        private void ApplyFilter()
        {
            _currentFilter = _filterService.GetCriteria(TbId, TbFirstName, TbLastName, TbMiddleName, TbCity, TbCountry, DpDateFrom, DpDateTo);
            _pagination.Reset();
            LoadPage();
        }

        /// <summary>
        /// Очищает все поля фильтрации и применяет пустой фильтр.
        /// </summary>
        private void ClearFilters()
        {
            _filterService.ClearFilters(
                TbId, TbFirstName, TbLastName, TbMiddleName,
                TbCity, TbCountry, DpDateFrom, DpDateTo);
            ApplyFilter();
        }

        /// <summary>
        /// Включает/отключает элементы управления и меняет курсор для индикации длительной операции.
        /// </summary>
        /// <param name="enabled">Состояние доступности.</param>
        private void SetControlsEnabled(bool enabled)
        {
            BtnLoad.IsEnabled = enabled;
            BtnApplyFilter.IsEnabled = enabled;
            BtnClearFilter.IsEnabled = enabled;
            BtnExportExcel.IsEnabled = enabled;
            BtnExportXml.IsEnabled = enabled;
            BtnPrevPage.IsEnabled = enabled && _pagination.CanGoPrevious;
            BtnNextPage.IsEnabled = enabled && _pagination.CanGoNext;
            this.Cursor = enabled ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Wait;
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            _pagination.PrevPage();
            LoadPage();
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            _pagination.NextPage();
            LoadPage();
        }

        private void TbPageNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (int.TryParse(TbPageNumber.Text, out int page))
                {
                    _pagination.SetCurrentPage(page);
                    LoadPage();
                }
                else
                {
                    TbPageNumber.Text = _pagination.CurrentPage.ToString();
                }
            }
        }

        private async void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SetControlsEnabled(false);
                TbStatus.Text = "Импорт данных...";

                var (count, error) = await _importService.ImportAsync(openFileDialog.FileName);

                if (string.IsNullOrEmpty(error))
                {
                    ApplyFilter();
                    TbStatus.Text = $"✅ Успешно загружено {count} записей.";
                }
                else
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    TbStatus.Text = "❌ Ошибка импорта.";
                }

                SetControlsEnabled(true);
            }
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e) => ApplyFilter();
        private void BtnClearFilter_Click(object sender, RoutedEventArgs e) => ClearFilters();

        /// <summary>
        /// Обобщённый метод экспорта данных с выбором файла и вызовом переданного действия.
        /// Теперь принимает Action&lt;IEnumerable&lt;Person&gt;, string&gt; для потоковой обработки.
        /// </summary>
        /// <param name="extension">Расширение файла.</param>
        /// <param name="filterDescription">Описание фильтра для диалога сохранения.</param>
        /// <param name="exportAction">Действие экспорта, принимающее поток данных и путь.</param>
        private async void ExportData(string extension, string filterDescription, Action<IEnumerable<Person>, string> exportAction)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{filterDescription} (*.{extension})|*.{extension}|All files (*.*)|*.*",
                DefaultExt = $".{extension}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                SetControlsEnabled(false);
                TbStatus.Text = "Экспорт...";

                var (count, error) = await _exportService.ExportAsync(_currentFilter, saveDialog.FileName, exportAction);

                if (string.IsNullOrEmpty(error))
                {
                    TbStatus.Text = $"✅ Экспортировано {count} записей в {saveDialog.FileName}";
                }
                else
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    TbStatus.Text = "❌ Ошибка экспорта.";
                }
                SetControlsEnabled(true);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e) => ExportData("xlsx", "Excel files", _excelExporter.Export);
        private void BtnExportXml_Click(object sender, RoutedEventArgs e) => ExportData("xml", "XML files", _xmlExporter.Export);
    }
}