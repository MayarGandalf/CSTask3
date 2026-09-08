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
    /// <summary>
    /// Тесты для <see cref="ImportService"/>, проверяющие логику импорта с использованием репозитория.
    /// </summary>
    public class ImportServiceTests
    {
        /// <summary>
        /// Проверяет, что при импорте валидных данных все записи добавляются в репозиторий и выполняется сброс буфера.
        /// </summary>
        [Fact]
        public async Task ImportValidDataShouldAddAndFlush()
        {
            var importerMock = new Mock<Importer>();
            var repositoryMock = new Mock<PersonManager>();

            var persons = new[]
            {
                new Person { Id = 1, FirstName = "John" },
                new Person { Id = 2, FirstName = "Jane" }
            };
            importerMock.Setup(importer => importer.ImportAsync(It.IsAny<string>())).Returns(persons.ToAsyncEnumerable());

            var service = new ImportService(importerMock.Object, repositoryMock.Object);
            var (count, error) = await service.ImportAsync("test.csv");

            Assert.Null(error);
            Assert.Equal(2, count);
            repositoryMock.Verify(repository => repository.AddForBulkAsync(It.IsAny<Person>()), Times.Exactly(2));
            repositoryMock.Verify(repository => repository.FlushBulkAsync(), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при отсутствии данных возвращается соответствующая ошибка,
        /// и метод добавления не вызывается.
        /// </summary>
        [Fact]
        public async Task ImportWithNoDataReturnsError()
        {
            var importerMock = new Mock<Importer>();
            var repositoryMock = new Mock<PersonManager>();
            importerMock.Setup(importer => importer.ImportAsync(It.IsAny<string>())).Returns(AsyncEnumerable.Empty<Person>());

            var service = new ImportService(importerMock.Object, repositoryMock.Object);
            var (count, error) = await service.ImportAsync("test.csv");

            Assert.Equal(0, count);
            Assert.Equal("Не найдено корректных данных.", error);
            repositoryMock.Verify(repository => repository.AddForBulkAsync(It.IsAny<Person>()), Times.Never);
            repositoryMock.Verify(repository => repository.FlushBulkAsync(), Times.Once);
        }

        /// <summary>
        /// Проверяет, что если <see cref="Importer"/> выбрасывает исключение,
        /// сервис возвращает ошибку и не вызывает методы репозитория.
        /// </summary>
        [Fact]
        public async Task ImporterThrowsThenServiceReturnsError()
        {
            var importerMock = new Mock<Importer>();
            var repositoryMock = new Mock<PersonManager>();
            importerMock.Setup(importer => importer.ImportAsync(It.IsAny<string>())).Throws(new InvalidOperationException("File read error"));

            var service = new ImportService(importerMock.Object, repositoryMock.Object);
            var (count, error) = await service.ImportAsync("test.csv");

            Assert.Equal(0, count);
            Assert.Contains("Ошибка импорта", error);
            Assert.Contains("File read error", error);
            repositoryMock.Verify(repository => repository.AddForBulkAsync(It.IsAny<Person>()), Times.Never);
            repositoryMock.Verify(repository => repository.FlushBulkAsync(), Times.Never);
        }

        /// <summary>
        /// Проверяет, что если репозиторий выбрасывает исключение при добавлении,
        /// сервис возвращает ошибку и не вызывает <see cref="PersonManager.FlushBulkAsync"/>.
        /// </summary>
        [Fact]
        public async Task RepositoryAddFailsThenReturnsErrorAndClearsBuffer()
        {
            var importerMock = new Mock<Importer>();
            var repositoryMock = new Mock<PersonManager>();

            var persons = new[] { new Person { Id = 1, FirstName = "John" } };
            importerMock.Setup(importer => importer.ImportAsync(It.IsAny<string>())).Returns(persons.ToAsyncEnumerable());
            repositoryMock.Setup(repository => repository.AddForBulkAsync(It.IsAny<Person>())).ThrowsAsync(new InvalidOperationException("DB insert error"));

            var service = new ImportService(importerMock.Object, repositoryMock.Object);
            var (count, error) = await service.ImportAsync("test.csv");

            Assert.Equal(0, count);
            Assert.Contains("Ошибка импорта", error);
            Assert.Contains("DB insert error", error);
            repositoryMock.Verify(repository => repository.AddForBulkAsync(It.IsAny<Person>()), Times.Once);
            repositoryMock.Verify(repository => repository.FlushBulkAsync(), Times.Never);
        }
    }
}