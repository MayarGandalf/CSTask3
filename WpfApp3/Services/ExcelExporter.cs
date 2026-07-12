using FastExcelWriterStream;
using System.Collections.Generic;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    /// <summary>
    /// Экспорт данных в файл Excel (XLSX) с использованием библиотеки FastExcelWriterStream.
    /// </summary>
    public class ExcelExporter
    {
        /// <summary>
        /// Сохраняет перечисление Person в Excel-файл по указанному пути.
        /// </summary>
        /// <param name="data">Данные для экспорта.</param>
        /// <param name="filePath">Путь к выходному файлу.</param>
        public void Export(IEnumerable<Person> data, string filePath)
        {
            using var excelWriter = new ExcelWriter(filePath);

            excelWriter.WriteRow("Id", "Date", "FirstName", "LastName", "MiddleName", "City", "Country");

            foreach (var person in data)
            {
                excelWriter.WriteRow(
                    person.Id,
                    person.Date.ToString("dd.MM.yyyy"),
                    person.FirstName ?? string.Empty,
                    person.LastName ?? string.Empty,
                    person.MiddleName ?? string.Empty,
                    person.City ?? string.Empty,
                    person.Country ?? string.Empty
                );
            }
        }
    }
}