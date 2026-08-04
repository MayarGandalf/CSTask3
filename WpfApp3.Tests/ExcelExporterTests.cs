using System.Collections.Generic;
using System.IO;
using WpfApp3.Models;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    public class ExcelExporterTests
    {
        [Fact]
        public void CreateFile()
        {
            var exporter = new ExcelExporter();
            var testData = new List<Person>
            {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Date = new System.DateTime(2020, 1, 1)
                }
            };
            string tempFile = Path.GetTempFileName() + ".xlsx";

            try
            {
                exporter.Export(testData, tempFile);
                Assert.True(File.Exists(tempFile));
                Assert.True(new FileInfo(tempFile).Length > 0);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void EmptyData()
        {
            var exporter = new ExcelExporter();
            var emptyData = new List<Person>();
            string tempFile = Path.GetTempFileName() + ".xlsx";

            try
            {
                exporter.Export(emptyData, tempFile);
                Assert.True(File.Exists(tempFile));
                Assert.True(new FileInfo(tempFile).Length > 0);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}