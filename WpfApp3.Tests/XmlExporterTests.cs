using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using WpfApp3.Models;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    public class XmlExporterTests
    {
        [Fact]
        public void CreateXmlWithStructure()
        {
            var exporter = new XmlExporter();
            var testData = new List<Person>
            {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    MiddleName = "M",
                    City = "NY",
                    Country = "USA",
                    Date = new System.DateTime(2020, 1, 1)
                }
            };
            string tempFile = Path.GetTempFileName() + ".xml";

            try
            {
                exporter.Export(testData, tempFile);

                Assert.True(File.Exists(tempFile));
                var doc = XDocument.Load(tempFile);
                var root = doc.Root;
                Assert.NotNull(root);
                Assert.Equal("Persons", root.Name.LocalName);

                var records = root.Element("Records");
                Assert.NotNull(records);

                var person = records.Element("Person");
                Assert.NotNull(person);

                var idElement = person.Element("Id");
                Assert.NotNull(idElement);
                Assert.Equal("1", idElement.Value);

                var dateElement = person.Element("Date");
                Assert.NotNull(dateElement);
                Assert.Equal("2020-01-01", dateElement.Value);

                var firstNameElement = person.Element("FirstName");
                Assert.NotNull(firstNameElement);
                Assert.Equal("John", firstNameElement.Value);

                var lastNameElement = person.Element("LastName");
                Assert.NotNull(lastNameElement);
                Assert.Equal("Doe", lastNameElement.Value);

                var middleNameElement = person.Element("MiddleName");
                Assert.NotNull(middleNameElement);
                Assert.Equal("M", middleNameElement.Value);

                var cityElement = person.Element("City");
                Assert.NotNull(cityElement);
                Assert.Equal("NY", cityElement.Value);

                var countryElement = person.Element("Country");
                Assert.NotNull(countryElement);
                Assert.Equal("USA", countryElement.Value);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void EmptyDataXml()
        {
            var exporter = new XmlExporter();
            var emptyData = new List<Person>();
            string tempFile = Path.GetTempFileName() + ".xml";

            try
            {
                exporter.Export(emptyData, tempFile);
                Assert.True(File.Exists(tempFile));
                var doc = XDocument.Load(tempFile);
                var root = doc.Root;
                Assert.NotNull(root);
                var records = root.Element("Records");
                Assert.NotNull(records);
                Assert.Empty(records.Elements());
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}