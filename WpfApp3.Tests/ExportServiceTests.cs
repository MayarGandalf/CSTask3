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
    /// <summary>
    /// Тесты для <see cref="ExportService"/>, проверяющие логику экспорта данных.
    /// </summary>
    public class ExportServiceTests
    {
        /// <summary>
        /// Проверяет, что при наличии данных вызывается переданное действие экспорта
        /// и возвращается корректное количество записей.
        /// </summary>
        [Fact]
        public async Task ExportWithDataCallsActionAndReturnsCount()
        {
            var repositoryMock = new Mock<PersonManager>();
            var persons = new List<Person>
            {
                new Person { Id = 1, FirstName = "John" },
                new Person { Id = 2, FirstName = "Jane" }
            };
            repositoryMock.Setup(repository => repository.GetTotalCount(It.IsAny<FilterCriteria>())).Returns(persons.Count);
            repositoryMock.Setup(repository => repository.GetFilteredStream(It.IsAny<FilterCriteria>())).Returns(persons);

            var service = new ExportService(repositoryMock.Object);
            bool exportCalled = false;

            var (count, error) = await service.ExportAsync(new FilterCriteria(), "test.xlsx",
                (data, path) =>
                {
                    exportCalled = true;
                    Assert.Equal(2, data.Count());
                });

            Assert.Null(error);
            Assert.Equal(2, count);
            Assert.True(exportCalled);
            repositoryMock.Verify(repository => repository.GetTotalCount(It.IsAny<FilterCriteria>()), Times.Once);
            repositoryMock.Verify(repository => repository.GetFilteredStream(It.IsAny<FilterCriteria>()), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при отсутствии данных возвращается ошибка и действие экспорта не вызывается.
        /// </summary>
        [Fact]
        public async Task ExportWithNoDataReturnsErrorAndDoesNotExport()
        {
            var repositoryMock = new Mock<PersonManager>();
            repositoryMock.Setup(repository => repository.GetTotalCount(It.IsAny<FilterCriteria>())).Returns(0);

            var service = new ExportService(repositoryMock.Object);
            bool exportCalled = false;

            var (count, error) = await service.ExportAsync(new FilterCriteria(), "test.xlsx",
                (_, _) => exportCalled = true);

            Assert.Equal(0, count);
            Assert.Equal("Нет данных для экспорта с выбранными фильтрами.", error);
            Assert.False(exportCalled);
            repositoryMock.Verify(repository => repository.GetTotalCount(It.IsAny<FilterCriteria>()), Times.Once);
            repositoryMock.Verify(repository => repository.GetFilteredStream(It.IsAny<FilterCriteria>()), Times.Never);
        }

        /// <summary>
        /// Проверяет, что если репозиторий выбрасывает исключение, сервис возвращает ошибку с сообщением.
        /// </summary>
        [Fact]
        public async Task ExportWhenRepositoryThrows()
        {
            var repositoryMock = new Mock<PersonManager>();
            repositoryMock.Setup(repository => repository.GetTotalCount(It.IsAny<FilterCriteria>()))
                          .Throws(new InvalidOperationException("DB error"));

            var service = new ExportService(repositoryMock.Object);

            var (count, error) = await service.ExportAsync(new FilterCriteria(), "test.xlsx",
                (_, _) => { });

            Assert.Equal(0, count);
            Assert.Contains("Ошибка экспорта", error);
            Assert.Contains("DB error", error);
        }
    }
}