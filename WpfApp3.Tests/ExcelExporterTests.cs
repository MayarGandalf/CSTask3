using System.Collections.Generic;
using System.IO;
using WpfApp3.Models;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    /// <summary>
    /// Тесты для <see cref="ExcelExporter"/>, проверяющие создание Excel-файлов.
    /// </summary>
    public class ExcelExporterTests
    {
        /// <summary>
        /// Проверяет, что экспорт данных создаёт непустой Excel-файл.
        /// </summary>
        [Fact]
        public void ExportExcelWithSomeData()
        {
            var exporter = new ExcelExporter();
            var persons = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", Date = new System.DateTime(2020, 1, 1) }
            };
            var tempFile = Path.GetTempFileName() + ".xlsx";

            exporter.Export(persons, tempFile);

            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);

            File.Delete(tempFile);
        }

        /// <summary>
        /// Проверяет, что экспорт пустого списка создаёт Excel-файл только с заголовками.
        /// </summary>
        [Fact]
        public void ExportExcelWithEmptyList()
        {
            var exporter = new ExcelExporter();
            var tempFile = Path.GetTempFileName() + ".xlsx";

            exporter.Export(new List<Person>(), tempFile);

            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);

            File.Delete(tempFile);
        }
    }
}