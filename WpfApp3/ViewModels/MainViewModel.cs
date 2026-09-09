using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp3.Commands;
using WpfApp3.Models;
using WpfApp3.Services;
using WpfApp3.Helpers;

namespace WpfApp3.ViewModels
{
    /// <summary>
    /// Главная модель представления приложения, управляющая отображением данных,
    /// фильтрацией, пагинацией, импортом и экспортом.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly PersonManager _repository;
        private readonly PaginationService _pagination;
        private readonly ExportService _exportService;
        private readonly ImportService _importService;
        private readonly ExcelExporter _excelExporter;
        private readonly XmlExporter _xmlExporter;

        private ObservableCollection<Person> _persons = new();

        private string? _filterId;
        private string? _filterFirstName;
        private string? _filterLastName;
        private string? _filterMiddleName;
        private string? _filterCity;
        private string? _filterCountry;
        private DateTime? _filterDateFrom;
        private DateTime? _filterDateTo;
        private int _currentPage;
        private int _totalPages;
        private int _totalRecords;
        private string _statusText = "Готово";
        private bool _isBusy;

        /// <summary>
        /// Возвращает или задаёт коллекцию отображаемых записей Person.
        /// </summary>
        public ObservableCollection<Person> Persons
        {
            get => _persons;
            set => SetProperty(ref _persons, value);
        }

        /// <summary>
        /// Возвращает или задаёт значение фильтра по идентификатору.
        /// </summary>
        public string? FilterId
        {
            get => _filterId;
            set => SetProperty(ref _filterId, value);
        }

        /// <summary>
        /// Возвращает или задаёт значение фильтра по имени.
        /// </summary>
        public string? FilterFirstName
        {
            get => _filterFirstName;
            set => SetProperty(ref _filterFirstName, value);
        }

        /// <summary>
        /// Возвращает или задаёт значение фильтра по фамилии.
        /// </summary>
        public string? FilterLastName
        {
            get => _filterLastName;
            set => SetProperty(ref _filterLastName, value);
        }

        /// <summary>
        /// Возвращает или задаёт значение фильтра по отчеству.
        /// </summary>
        public string? FilterMiddleName
        {
            get => _filterMiddleName;
            set => SetProperty(ref _filterMiddleName, value);
        }

        /// <summary>
        /// Возвращает или задаёт значение фильтра по городу.
        /// </summary>
        public string? FilterCity
        {
            get => _filterCity;
            set => SetProperty(ref _filterCity, value);
        }

        /// <summary>
        /// Возвращает или задаёт значение фильтра по стране.
        /// </summary>
        public string? FilterCountry
        {
            get => _filterCountry;
            set => SetProperty(ref _filterCountry, value);
        }

        /// <summary>
        /// Возвращает или задаёт начальную дату фильтрации.
        /// </summary>
        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set => SetProperty(ref _filterDateFrom, value);
        }

        /// <summary>
        /// Возвращает или задаёт конечную дату фильтрации.
        /// </summary>
        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set => SetProperty(ref _filterDateTo, value);
        }

        /// <summary>
        /// Возвращает или задаёт номер текущей страницы.
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        /// <summary>
        /// Возвращает или задаёт общее количество страниц.
        /// </summary>
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        /// <summary>
        /// Возвращает или задаёт общее количество записей.
        /// </summary>
        public int TotalRecords
        {
            get => _totalRecords;
            set => SetProperty(ref _totalRecords, value);
        }

        /// <summary>
        /// Возвращает или задаёт текст состояния, отображаемый в интерфейсе.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Возвращает или задаёт признак занятости приложения (выполняется длительная операция).
        /// При изменении вызывает принудительное обновление команд.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>Команда загрузки данных из CSV-файла.</summary>
        public ICommand LoadCsvCommand { get; }

        /// <summary>Команда применения фильтра.</summary>
        public ICommand ApplyFilterCommand { get; }

        /// <summary>Команда сброса фильтра.</summary>
        public ICommand ClearFilterCommand { get; }

        /// <summary>Команда экспорта данных в Excel.</summary>
        public ICommand ExportExcelCommand { get; }

        /// <summary>Команда экспорта данных в XML.</summary>
        public ICommand ExportXmlCommand { get; }

        /// <summary>Команда перехода на следующую страницу.</summary>
        public ICommand NextPageCommand { get; }

        /// <summary>Команда перехода на предыдущую страницу.</summary>
        public ICommand PrevPageCommand { get; }

        /// <summary>Команда перехода на страницу с указанным номером.</summary>
        public ICommand GoToPageCommand { get; }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="MainViewModel"/>,
        /// создаёт сервисы и команды, загружает первую страницу данных.
        /// </summary>
        public MainViewModel()
        {
            _repository = new PersonManager();
            _pagination = new PaginationService(100);
            _excelExporter = new ExcelExporter();
            _xmlExporter = new XmlExporter();
            _exportService = new ExportService(_repository);
            _importService = new ImportService(new Importer(), _repository);

            CurrentPage = _pagination.CurrentPage;
            TotalPages = _pagination.TotalPages;
            TotalRecords = _pagination.TotalRecords;

            LoadCsvCommand = new CommandsWorker(ExecuteLoadCsv, () => !IsBusy);
            ApplyFilterCommand = new CommandsWorker(ExecuteApplyFilter, () => !IsBusy);
            ClearFilterCommand = new CommandsWorker(ExecuteClearFilter, () => !IsBusy);
            ExportExcelCommand = new CommandsWorker(ExecuteExportExcel, () => !IsBusy);
            ExportXmlCommand = new CommandsWorker(ExecuteExportXml, () => !IsBusy);
            NextPageCommand = new CommandsWorker(ExecuteNextPage, () => !IsBusy && _pagination.CanGoNext);
            PrevPageCommand = new CommandsWorker(ExecutePrevPage, () => !IsBusy && _pagination.CanGoPrevious);
            GoToPageCommand = new RelayCommand<string>(ExecuteGoToPage, _ => !IsBusy);

            _ = LoadPageAsync();
        }

        /// <summary>
        /// Формирует объект критериев фильтрации на основе текущих значений свойств.
        /// </summary>
        /// <returns>Экземпляр <see cref="FilterCriteria"/> с заполненными полями.</returns>
        private FilterCriteria BuildCriteria()
        {
            return new FilterCriteria
            {
                Id = int.TryParse(FilterId, out int id) ? id : (int?)null,
                FirstName = FilterFirstName,
                LastName = FilterLastName,
                MiddleName = FilterMiddleName,
                City = FilterCity,
                Country = FilterCountry,
                DateFrom = FilterDateFrom,
                DateTo = FilterDateTo
            };
        }

        /// <summary>
        /// Асинхронно загружает текущую страницу данных с учётом фильтра.
        /// Обновляет коллекцию <see cref="Persons"/> и счётчики пагинации.
        /// </summary>
        private async Task LoadPageAsync()
        {
            IsBusy = true;
            StatusText = "Загрузка...";

            try
            {
                var criteria = BuildCriteria();
                var dataTask = Task.Run(() => _repository.GetPaged(_pagination.Skip, _pagination.PageSize, criteria));
                var totalTask = Task.Run(() => _repository.GetTotalCount(criteria));

                await Task.WhenAll(dataTask, totalTask);
                var data = await dataTask;
                var total = await totalTask;

                _pagination.SetTotalRecords(total);
                CurrentPage = _pagination.CurrentPage;
                TotalPages = _pagination.TotalPages;
                TotalRecords = total;

                Persons.Clear();
                foreach (var person in data)
                    Persons.Add(person);

                StatusText = $"Показано: {data.Count} (всего: {total})";
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка загрузки страницы");
                StatusText = $"❌ Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Выполняет команду загрузки CSV-файла: открывает диалог выбора файла и запускает импорт.
        /// </summary>
        private void ExecuteLoadCsv()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv"
            };
            if (dialog.ShowDialog() == true)
            {
                _ = ImportAsync(dialog.FileName);
            }
        }

        /// <summary>
        /// Асинхронно импортирует данные из CSV-файла и обновляет отображение.
        /// </summary>
        /// <param name="filePath">Путь к импортируемому файлу.</param>
        private async Task ImportAsync(string filePath)
        {
            IsBusy = true;
            StatusText = "Импорт...";

            var (count, error) = await _importService.ImportAsync(filePath);
            if (string.IsNullOrEmpty(error))
            {
                StatusText = $"✅ Успешно загружено {count} записей.";
                _pagination.Reset();
                await LoadPageAsync();
            }
            else
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "❌ Ошибка импорта.";
            }
            IsBusy = false;
        }

        /// <summary>
        /// Применяет текущие фильтры и перезагружает данные.
        /// </summary>
        private async void ExecuteApplyFilter()
        {
            await LoadPageAsync();
        }

        /// <summary>
        /// Сбрасывает все фильтры и перезагружает данные.
        /// </summary>
        private async void ExecuteClearFilter()
        {
            FilterId = null;
            FilterFirstName = null;
            FilterLastName = null;
            FilterMiddleName = null;
            FilterCity = null;
            FilterCountry = null;
            FilterDateFrom = null;
            FilterDateTo = null;
            await LoadPageAsync();
        }

        /// <summary>
        /// Запускает экспорт в Excel через диалог сохранения файла.
        /// </summary>
        private void ExecuteExportExcel() => ExportData("xlsx", "Excel files", _excelExporter.Export);

        /// <summary>
        /// Запускает экспорт в XML через диалог сохранения файла.
        /// </summary>
        private void ExecuteExportXml() => ExportData("xml", "XML files", _xmlExporter.Export);

        /// <summary>
        /// Обобщённый метод экспорта данных с выбором формата.
        /// </summary>
        /// <param name="extension">Расширение файла.</param>
        /// <param name="filterDescription">Описание фильтра для диалога сохранения.</param>
        /// <param name="exportAction">Делегат, выполняющий фактический экспорт.</param>
        private async void ExportData(string extension, string filterDescription, Action<IEnumerable<Person>, string> exportAction)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{filterDescription} (*.{extension})|*.{extension}|All files (*.*)|*.*",
                DefaultExt = $".{extension}"
            };
            if (saveDialog.ShowDialog() == true)
            {
                IsBusy = true;
                StatusText = "Экспорт...";

                var criteria = BuildCriteria();
                var (count, error) = await _exportService.ExportAsync(criteria, saveDialog.FileName, exportAction);

                if (string.IsNullOrEmpty(error))
                    StatusText = $"✅ Экспортировано {count} записей в {saveDialog.FileName}";
                else
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText = "❌ Ошибка экспорта.";
                }
                IsBusy = false;
            }
        }

        /// <summary>
        /// Переход на следующую страницу.
        /// </summary>
        private async void ExecuteNextPage()
        {
            _pagination.NextPage();
            await LoadPageAsync();
        }

        /// <summary>
        /// Переход на предыдущую страницу.
        /// </summary>
        private async void ExecutePrevPage()
        {
            _pagination.PrevPage();
            await LoadPageAsync();
        }

        /// <summary>
        /// Переход на страницу с заданным номером.
        /// </summary>
        /// <param name="pageStr">Номер страницы в виде строки.</param>
        private async void ExecuteGoToPage(string? pageStr)
        {
            if (int.TryParse(pageStr, out int page))
            {
                _pagination.SetCurrentPage(page);
                await LoadPageAsync();
            }
        }
    }
}