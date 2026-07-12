using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    /// <summary>
    /// Импорт данных из CSV-файла в перечисление объектов Person.
    /// Ожидается формат: Date;FirstName;LastName;MiddleName;City;Country с разделителем ';'.
    /// Дата может быть в формате dd.MM.yyyy или yyyy-MM-dd.
    /// Реализован потоковый асинхронный импорт без загрузки всего файла в память.
    /// </summary>
    public class Importer
    {
        /// <summary>
        /// Асинхронно читает CSV-файл и возвращает перечисление Person (потоково).
        /// Некорректные строки пропускаются.
        /// </summary>
        /// <param name="filePath">Путь к CSV-файлу.</param>
        /// <returns>Асинхронное перечисление объектов Person.</returns>
        public async IAsyncEnumerable<Person> ImportAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');
                if (parts.Length != 6)
                    continue;

                if (!DateTime.TryParseExact(parts[0].Trim(),
                        new[] { "dd.MM.yyyy", "yyyy-MM-dd" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                {
                    continue;
                }

                yield return new Person
                {
                    Date = date,
                    FirstName = parts[1].Trim(),
                    LastName = parts[2].Trim(),
                    MiddleName = parts[3].Trim(),
                    City = parts[4].Trim(),
                    Country = parts[5].Trim()
                };
            }
        }
    }
}