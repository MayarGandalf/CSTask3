using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp3.Models;
using WpfApp3.Helpers;

namespace WpfApp3.Services
{
    /// <summary>
    /// Сервис для экспорта отфильтрованных данных. Работает потоково, не загружая данные в память.
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
        /// экспортирует данные, отфильтрованные по критериям, с помощью переданного действия.
        /// </summary>
        /// <param name="criteria">Критерии фильтрации.</param>
        /// <param name="filePath">Путь к файлу для экспорта.</param>
        /// <param name="exportAction">Асинхронное действие записи данных в файл.</param>
        /// <returns>Кортеж: количество экспортированных записей и сообщение об ошибке.</returns>
        public async Task<(int count, string? error)> ExportAsync(
            FilterCriteria criteria,
            string filePath,
            Func<IAsyncEnumerable<Person>, string, Task> exportAction)
        {
            Logger.Info($"Начало экспорта в {filePath}");

            try
            {
                var totalCount = await _repository.GetTotalCountAsync(criteria).ConfigureAwait(false);
                if (totalCount == 0)
                {
                    Logger.Warning("Нет данных для экспорта по заданным фильтрам.");
                    return (0, "Нет данных для экспорта с выбранными фильтрами.");
                }

                var dataStream = _repository.GetFilteredStreamAsync(criteria);
                await exportAction(dataStream, filePath).ConfigureAwait(false);

                Logger.Info($"Экспорт завершён. Экспортировано {totalCount} записей.");
                return (totalCount, null);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Ошибка экспорта");
                return (0, $"Ошибка экспорта: {exception.Message}");
            }
        }
    }
}