using System;
using System.Threading.Tasks;
using WpfApp3.Models;
using WpfApp3.Helpers;

namespace WpfApp3.Services
{
    /// <summary>
    /// Сервис для импорта данных из CSV-файла в базу данных.
    /// Использует буферизированную массовую вставку для оптимизации производительности.
    /// </summary>
    public class ImportService
    {
        private readonly Importer _csvImporter;
        private readonly PersonManager _repository;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса импорта.
        /// </summary>
        /// <param name="csvImporter">Объект для чтения CSV.</param>
        /// <param name="repository">Репозиторий для сохранения данных.</param>
        public ImportService(Importer csvImporter, PersonManager repository)
        {
            _csvImporter = csvImporter;
            _repository = repository;
        }

        /// <summary>
        /// Асинхронно импортирует данные из CSV-файла, сохраняя их в БД.
        /// Записи передаются в репозиторий по одной для буферизации и пакетной вставки.
        /// </summary>
        /// <param name="filePath">Путь к CSV-файлу.</param>
        /// <returns>Кортеж: количество импортированных записей и сообщение об ошибке (null при успехе).</returns>
        public async Task<(int count, string? error)> ImportAsync(string filePath)
        {
            Logger.Info($"Import started from {filePath}");

            try
            {
                int totalCount = 0;
                int skippedCount = 0;
                await foreach (var person in _csvImporter.ImportAsync(filePath).ConfigureAwait(false))
                {
                    if (person == null)
                    {
                        skippedCount++;
                        continue;
                    }
                    await _repository.AddForBulkAsync(person).ConfigureAwait(false);
                    totalCount++;
                }

                await _repository.FlushBulkAsync().ConfigureAwait(false);

                Logger.Info($"Import completed. {totalCount} records loaded, {skippedCount} skipped.");

                if (totalCount == 0)
                    return (0, "Не найдено корректных данных.");

                return (totalCount, null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Import error");
                _repository.ClearBuffer();
                return (0, $"Ошибка импорта: {ex.Message}");
            }
        }
    }
}