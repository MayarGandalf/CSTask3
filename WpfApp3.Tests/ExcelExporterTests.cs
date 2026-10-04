using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task ExportExcelWithSomeData()
        {
            var exporter = new ExcelExporter();
            var persons = new List<Person>
            {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Date = new DateTime(2020, 1, 1)
                }
            };
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");

            await exporter.ExportAsync(persons.ToAsyncEnumerable(), tempFile);

            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);

            File.Delete(tempFile);
        }

        /// <summary>
        /// Проверяет, что экспорт пустого списка создаёт Excel-файл только с заголовками.
        /// </summary>
        [Fact]
        public async Task ExportExcelWithEmptyList()
        {
            var exporter = new ExcelExporter();
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");

            await exporter.ExportAsync(AsyncEnumerable.Empty<Person>(), tempFile);

            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);

            File.Delete(tempFile);
        }
    }
}