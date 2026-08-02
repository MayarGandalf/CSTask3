using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp3.Models;

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
            try
            {
                // Быстрый подсчёт общего количества записей (без загрузки данных)
                var totalCount = _repository.GetTotalCount(criteria);
                if (totalCount == 0)
                    return (0, "Нет данных для экспорта с выбранными фильтрами.");

                // Получаем потоковый перечислитель данных
                var dataStream = _repository.GetFilteredStream(criteria);

                // Экспортируем в фоновом потоке
                await Task.Run(() => exportAction(dataStream, filePath));

                return (totalCount, null);
            }
            catch (Exception exception)
            {
                return (0, $"Ошибка экспорта: {exception.Message}");
            }
        }
    }
}