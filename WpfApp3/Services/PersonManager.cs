using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using WpfApp3.Data;
using WpfApp3.Models;
using WpfApp3.Helpers;

namespace WpfApp3.Services
{
    /// <summary>
    /// Репозиторий для работы с сущностями Person в базе данных.
    /// </summary>
    public class PersonManager
    {
        private readonly List<Person> _bulkBuffer = new List<Person>(capacity: 50000);
        private readonly int _batchSize = 50000;

        /// <summary>
        /// Асинхронно возвращает одну страницу записей, отфильтрованных по критериям.
        /// </summary>
        public virtual async Task<List<Person>> GetPagedAsync(int skip, int take, FilterCriteria criteria)
        {
            await using var context = new ProjectDbContext();
            var query = BuildFilteredQuery(context.Persons.AsNoTracking(), criteria);
            return await query
                .OrderBy(person => person.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Асинхронно возвращает общее количество записей, удовлетворяющих критериям.
        /// </summary>
        public virtual async Task<int> GetTotalCountAsync(FilterCriteria criteria)
        {
            await using var context = new ProjectDbContext();
            var query = BuildFilteredQuery(context.Persons, criteria);
            return await query.CountAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Асинхронно возвращает поток записей без загрузки всего набора в память.
        /// </summary>
        public virtual async IAsyncEnumerable<Person> GetFilteredStreamAsync(FilterCriteria criteria)
        {
            await using var context = new ProjectDbContext();
            var query = BuildFilteredQuery(context.Persons.AsNoTracking(), criteria);
            query = query.OrderBy(person => person.Id);

            await foreach (var person in query.AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return person;
            }
        }

        /// <summary>
        /// Применяет критерии фильтрации к запросу.
        /// </summary>
        private IQueryable<Person> BuildFilteredQuery(IQueryable<Person> query, FilterCriteria criteria)
        {
            if (criteria.Id.HasValue)
                query = query.Where(person => person.Id == criteria.Id.Value);
            if (!string.IsNullOrWhiteSpace(criteria.FirstName))
                query = query.Where(person => person.FirstName != null && person.FirstName.Contains(criteria.FirstName));
            if (!string.IsNullOrWhiteSpace(criteria.LastName))
                query = query.Where(person => person.LastName != null && person.LastName.Contains(criteria.LastName));
            if (!string.IsNullOrWhiteSpace(criteria.MiddleName))
                query = query.Where(person => person.MiddleName != null && person.MiddleName.Contains(criteria.MiddleName));
            if (!string.IsNullOrWhiteSpace(criteria.City))
                query = query.Where(person => person.City != null && person.City.Contains(criteria.City));
            if (!string.IsNullOrWhiteSpace(criteria.Country))
                query = query.Where(person => person.Country != null && person.Country.Contains(criteria.Country));
            if (criteria.DateFrom.HasValue)
                query = query.Where(person => person.Date >= criteria.DateFrom.Value);
            if (criteria.DateTo.HasValue)
                query = query.Where(person => person.Date <= criteria.DateTo.Value);
            return query;
        }

        /// <summary>
        /// Добавляет одного человека в буфер. При заполнении — массовая вставка.
        /// </summary>
        public virtual async Task AddForBulkAsync(Person person)
        {
            _bulkBuffer.Add(person);
            if (_bulkBuffer.Count >= _batchSize)
            {
                await FlushBulkAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// вставляет все накопленные в буфере записи в базу данных.
        /// </summary>
        public virtual async Task FlushBulkAsync()
        {
            if (_bulkBuffer.Count == 0) return;

            await using var context = new ProjectDbContext();
            try
            {
                var config = new BulkConfig
                {
                    SetOutputIdentity = false,
                    PreserveInsertOrder = false,
                    UseTempDB = false,
                    BatchSize = _batchSize
                };
                await context.BulkInsertAsync(_bulkBuffer, config).ConfigureAwait(false);
                _bulkBuffer.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка массовой вставки");
                _bulkBuffer.Clear();
                throw;
            }
        }

        /// <summary>
        /// Очищает буфер массовой вставки (используется при ошибке импорта).
        /// </summary>
        public virtual void ClearBuffer() => _bulkBuffer.Clear();
    }
}