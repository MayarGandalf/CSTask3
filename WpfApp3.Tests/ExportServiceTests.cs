using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using WpfApp3.Models;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    public class ExportServiceTests
    {
        [Fact]
        public async Task ExportData()
        {
            var mockRepo = new Mock<PersonManager>();
            var testData = new List<Person>
            {
                new Person { Id = 1, FirstName = "John" },
                new Person { Id = 2, FirstName = "Jane" }
            };

            mockRepo.Setup(r => r.GetTotalCount(It.IsAny<FilterCriteria>()))
                    .Returns(testData.Count);
            mockRepo.Setup(r => r.GetFilteredStream(It.IsAny<FilterCriteria>()))
                    .Returns(testData);

            var service = new ExportService(mockRepo.Object);
            bool exportCalled = false;

            void ExportAction(IEnumerable<Person> data, string path)
            {
                exportCalled = true;
                Assert.Equal("test.xlsx", path);
                Assert.Equal(2, data.Count());
            }

            var (count, error) = await service.ExportAsync(new FilterCriteria(), "test.xlsx", ExportAction);

            Assert.Null(error);
            Assert.Equal(2, count);
            Assert.True(exportCalled);
            mockRepo.Verify(r => r.GetTotalCount(It.IsAny<FilterCriteria>()), Times.Once);
            mockRepo.Verify(r => r.GetFilteredStream(It.IsAny<FilterCriteria>()), Times.Once);
        }

        [Fact]
        public async Task NoDataExport()
        {
            var mockRepo = new Mock<PersonManager>();
            mockRepo.Setup(r => r.GetTotalCount(It.IsAny<FilterCriteria>()))
                    .Returns(0);

            var service = new ExportService(mockRepo.Object);
            bool exportCalled = false;

            var (count, error) = await service.ExportAsync(new FilterCriteria(), "test.xlsx",
                (data, path) => { exportCalled = true; });

            Assert.Equal(0, count);
            Assert.Equal("Нет данных для экспорта с выбранными фильтрами.", error);
            Assert.False(exportCalled);
            mockRepo.Verify(r => r.GetTotalCount(It.IsAny<FilterCriteria>()), Times.Once);
            mockRepo.Verify(r => r.GetFilteredStream(It.IsAny<FilterCriteria>()), Times.Never);
        }

        [Fact]
        public async Task CatchExceptionExport()
        {
            var mockRepo = new Mock<PersonManager>();
            mockRepo.Setup(r => r.GetTotalCount(It.IsAny<FilterCriteria>()))
                    .Throws(new InvalidOperationException("DB error"));

            var service = new ExportService(mockRepo.Object);

            var (count, error) = await service.ExportAsync(new FilterCriteria(), "test.xlsx",
                (data, path) => { });

            Assert.Equal(0, count);
            Assert.Contains("Ошибка экспорта", error);
            Assert.Contains("DB error", error);
        }
    }
}