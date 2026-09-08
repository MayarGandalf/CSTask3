using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    /// <summary>
    /// Тесты для класса <see cref="Importer"/>, проверяющие корректность импорта CSV-файлов.
    /// </summary>
    public class ImporterTests
    {
        /// <summary>
        /// Проверяет, что строки с некорректной датой или неверным количеством полей пропускаются,
        /// а корректные строки импортируются.
        /// </summary>
        [Fact]
        public async Task ImportShouldSkipInvalidLines()
        {
            var csvContent = "invalid date;John;Doe;Middle;City;Country\n" +
                             "01.01.2020;John;Doe;Middle;City;Country\n" +
                             "01.01.2020;John;Doe;MissingFields";
            var filePath = "test_invalid.csv";
            File.WriteAllText(filePath, csvContent);

            var importer = new Importer();
            var result = await importer.ImportAsync(filePath).ToListAsync();

            Assert.Single(result);
            Assert.Equal(new System.DateTime(2020, 1, 1), result[0].Date);

            File.Delete(filePath);
        }

        /// <summary>
        /// Проверяет, что импорт из пустого файла возвращает пустую коллекцию.
        /// </summary>
        [Fact]
        public async Task ImportFromEmptyFileReturnsNothing()
        {
            var filePath = "test_empty.csv";
            File.WriteAllText(filePath, "");

            var importer = new Importer();
            var result = await importer.ImportAsync(filePath).ToListAsync();

            Assert.Empty(result);

            File.Delete(filePath);
        }

        /// <summary>
        /// Проверяет, что пустые строки в CSV пропускаются, а валидные строки импортируются.
        /// </summary>
        [Fact]
        public async Task ImportSkipsEmptyLinesAndImportsValid()
        {
            var csvContent = "01.01.2020;John;Doe;Middle;City;Country\n" +
                             "\n" +
                             "01.01.2020;Jane;Smith;;London;UK";
            var filePath = "test_empty_lines.csv";
            File.WriteAllText(filePath, csvContent);

            var importer = new Importer();
            var result = await importer.ImportAsync(filePath).ToListAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].FirstName);
            Assert.Equal("Jane", result[1].FirstName);

            File.Delete(filePath);
        }
    }
}