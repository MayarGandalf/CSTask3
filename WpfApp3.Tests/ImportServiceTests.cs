using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfApp3.Models;
using WpfApp3.Services;
using Xunit;

namespace WpfApp3.Tests
{
    public class ImportServiceTests
    {
        [Fact]
        public async Task AddAllRecords()
        {
            // Arrange
            var mockImporter = new Mock<Importer>();
            var mockRepository = new Mock<PersonManager>();

            var testData = new List<Person>
            {
                new Person { Id = 1, FirstName = "John" },
                new Person { Id = 2, FirstName = "Jane" }
            };

            mockImporter
                .Setup(x => x.ImportAsync(It.IsAny<string>()))
                .Returns(testData.ToAsyncEnumerable());

            var service = new ImportService(mockImporter.Object, mockRepository.Object);

            // Act
            var (count, error) = await service.ImportAsync("test.csv");

            // Assert
            Assert.Null(error);
            Assert.Equal(2, count);
            mockRepository.Verify(r => r.AddForBulkAsync(It.IsAny<Person>()), Times.Exactly(2));
            mockRepository.Verify(r => r.FlushBulkAsync(), Times.Once);
        }

        [Fact]
        public async Task NoData()
        {
            // Arrange
            var mockImporter = new Mock<Importer>();
            var mockRepository = new Mock<PersonManager>();

            mockImporter
                .Setup(x => x.ImportAsync(It.IsAny<string>()))
                .Returns(AsyncEnumerable.Empty<Person>());

            var service = new ImportService(mockImporter.Object, mockRepository.Object);

            // Act
            var (count, error) = await service.ImportAsync("test.csv");

            // Assert
            Assert.Equal(0, count);
            Assert.Equal("Не найдено корректных данных.", error);
            mockRepository.Verify(r => r.AddForBulkAsync(It.IsAny<Person>()), Times.Never);
            mockRepository.Verify(r => r.FlushBulkAsync(), Times.Once);
        }

        [Fact]
        public async Task CatchException()
        {
            // Arrange
            var mockImporter = new Mock<Importer>();
            var mockRepository = new Mock<PersonManager>();

            mockImporter
                .Setup(x => x.ImportAsync(It.IsAny<string>()))
                .Throws(new InvalidOperationException("File read error"));

            var service = new ImportService(mockImporter.Object, mockRepository.Object);

            // Act
            var (count, error) = await service.ImportAsync("test.csv");

            // Assert
            Assert.Equal(0, count);
            Assert.Contains("Ошибка импорта", error);
            Assert.Contains("File read error", error);
            mockRepository.Verify(r => r.AddForBulkAsync(It.IsAny<Person>()), Times.Never);
            mockRepository.Verify(r => r.FlushBulkAsync(), Times.Never);
        }

        [Fact]
        public async Task RepositoryFails()
        {
            // Arrange
            var mockImporter = new Mock<Importer>();
            var mockRepository = new Mock<PersonManager>();

            var testData = new List<Person>
            {
                new Person { Id = 1, FirstName = "John" }
            };

            mockImporter
                .Setup(x => x.ImportAsync(It.IsAny<string>()))
                .Returns(testData.ToAsyncEnumerable());

            mockRepository
                .Setup(r => r.AddForBulkAsync(It.IsAny<Person>()))
                .ThrowsAsync(new InvalidOperationException("DB insert error"));

            var service = new ImportService(mockImporter.Object, mockRepository.Object);

            // Act
            var (count, error) = await service.ImportAsync("test.csv");

            // Assert
            Assert.Equal(0, count);
            Assert.Contains("Ошибка импорта", error);
            Assert.Contains("DB insert error", error);
            mockRepository.Verify(r => r.AddForBulkAsync(It.IsAny<Person>()), Times.Once);
            mockRepository.Verify(r => r.FlushBulkAsync(), Times.Never);
        }

        [Fact]
        public async Task RepositorySucceeds()
        {
            // Arrange
            var mockImporter = new Mock<Importer>();
            var mockRepository = new Mock<PersonManager>();

            var testData = new List<Person>
            {
                new Person { Id = 1, FirstName = "John" },
                new Person { Id = 2, FirstName = "Jane" }
            };

            mockImporter
                .Setup(x => x.ImportAsync(It.IsAny<string>()))
                .Returns(testData.ToAsyncEnumerable());

            mockRepository
                .Setup(r => r.AddForBulkAsync(It.IsAny<Person>()))
                .Returns(Task.CompletedTask);

            var service = new ImportService(mockImporter.Object, mockRepository.Object);

            // Act
            var (count, error) = await service.ImportAsync("test.csv");

            // Assert
            Assert.Null(error);
            Assert.Equal(2, count);
            mockRepository.Verify(r => r.AddForBulkAsync(It.IsAny<Person>()), Times.Exactly(2));
            mockRepository.Verify(r => r.FlushBulkAsync(), Times.Once);
        }
    }
}