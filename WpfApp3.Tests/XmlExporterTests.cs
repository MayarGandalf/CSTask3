using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using WpfApp3.Models;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    /// <summary>
    /// Тесты для <see cref="XmlExporter"/>, проверяющие корректность генерации XML-файлов.
    /// </summary>
    public class XmlExporterTests
    {
        /// <summary>
        /// Проверяет, что экспорт данных создаёт XML-файл с правильной структурой и содержимым.
        /// </summary>
        [Fact]
        public async Task ExportXmlWithValidData()
        {
            var exporter = new XmlExporter();
            var persons = new[]
            {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    MiddleName = "M",
                    City = "NY",
                    Country = "USA",
                    Date = new DateTime(2020, 1, 1)
                }
            };
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");

            await exporter.ExportAsync(persons.ToAsyncEnumerable(), tempFile);

            Assert.True(File.Exists(tempFile));
            var document = XDocument.Load(tempFile);
            var root = document.Root;
            Assert.Equal("Persons", root.Name.LocalName);

            var records = root.Element("Records");
            var personElement = records.Element("Person");

            Assert.Equal("1", personElement.Element("Id").Value);
            Assert.Equal("2020-01-01", personElement.Element("Date").Value);
            Assert.Equal("John", personElement.Element("FirstName").Value);
            Assert.Equal("Doe", personElement.Element("LastName").Value);
            Assert.Equal("M", personElement.Element("MiddleName").Value);
            Assert.Equal("NY", personElement.Element("City").Value);
            Assert.Equal("USA", personElement.Element("Country").Value);

            File.Delete(tempFile);
        }

        /// <summary>
        /// Проверяет, что экспорт пустого набора данных создаёт XML-файл с пустым блоком Records.
        /// </summary>
        [Fact]
        public async Task ExportEmptyXmlCreatesEmptyRecords()
        {
            var exporter = new XmlExporter();
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");

            await exporter.ExportAsync(AsyncEnumerable.Empty<Person>(), tempFile);

            Assert.True(File.Exists(tempFile));
            var document = XDocument.Load(tempFile);
            var records = document.Root.Element("Records");
            Assert.Empty(records.Elements());

            File.Delete(tempFile);
        }
    }
}