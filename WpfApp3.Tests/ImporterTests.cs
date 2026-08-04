using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    public class ImporterTests
    {
        [Fact]
        public async Task SkipInvalidLines()
        {
            // Arrange
            var csvContent =
                "invalid date;John;Doe;Middle;City;Country\n" +
                "01.01.2020;John;Doe;Middle;City;Country\n" +
                "01.01.2020;John;Doe;MissingFields";
            var filePath = "test_invalid.csv";
            File.WriteAllText(filePath, csvContent);

            var importer = new Importer();

            // Act
            var result = await importer.ImportAsync(filePath).ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(new System.DateTime(2020, 1, 1), result[0].Date);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task EmptyFile()
        {
            // Arrange
            var csvContent = "";
            var filePath = "test_empty.csv";
            File.WriteAllText(filePath, csvContent);

            var importer = new Importer();

            // Act
            var result = await importer.ImportAsync(filePath).ToListAsync();

            // Assert
            Assert.Empty(result);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task SkipEmptyLines()
        {
            // Arrange
            var csvContent =
                "01.01.2020;John;Doe;Middle;City;Country\n" +
                "\n" +
                "01.01.2020;Jane;Smith;;London;UK";
            var filePath = "test_empty_lines.csv";
            File.WriteAllText(filePath, csvContent);

            var importer = new Importer();

            // Act
            var result = await importer.ImportAsync(filePath).ToListAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].FirstName);
            Assert.Equal("Jane", result[1].FirstName);

            // Cleanup
            File.Delete(filePath);
        }
    }
}