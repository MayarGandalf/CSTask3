using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp3.Models;
using WpfApp3.Helpers;

namespace WpfApp3.Services
{
    /// <summary>
    /// Сервис для экспорта отфильтрованных данных в файл (Excel, XML и т.д.).
    /// </summary>
    public class ExportService
    {
        private readonly PersonManager _repository;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса экспорта.
        /// </summary>
        /// <param name="repository">Репозиторий для доступа к данным.</param>
        public ExportService(PersonManager repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Асинхронно экспортирует данные, отфильтрованные по критериям, с помощью переданного действия.
        /// Данные передаются потоково, без загрузки всего набора в память.
        /// </summary>
        /// <param name="criteria">Критерии фильтрации.</param>
        /// <param name="filePath">Путь к файлу для экспорта.</param>
        /// <param name="exportAction">Действие, выполняющее фактическую запись данных в файл (принимает IEnumerable&lt;Person&gt;).</param>
        /// <returns>Кортеж: количество экспортированных записей и сообщение об ошибке (null, если успешно).</returns>
        public async Task<(int count, string? error)> ExportAsync(
            FilterCriteria criteria,
            string filePath,
            Action<IEnumerable<Person>, string> exportAction)
        {
            Logger.Info($"Export started to {filePath}");

            try
            {
                var totalCount = _repository.GetTotalCount(criteria);
                if (totalCount == 0)
                {
                    Logger.Warning("No data to export with the current filters.");
                    return (0, "Нет данных для экспорта с выбранными фильтрами.");
                }

                var dataStream = _repository.GetFilteredStream(criteria);
                await Task.Run(() => exportAction(dataStream, filePath));

                Logger.Info($"Export completed. {totalCount} records exported.");
                return (totalCount, null);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Export error");
                return (0, $"Ошибка экспорта: {exception.Message}");
            }
        }
    }
}