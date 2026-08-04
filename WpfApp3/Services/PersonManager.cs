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
    /// Обеспечивает фильтрацию, пагинацию, массовую вставку с буферизацией для быстрого импорта.
    /// </summary>
    public class PersonManager
    {
        private readonly List<Person> _bulkBuffer = new List<Person>(capacity: 50000);
        private readonly int _batchSize = 50000;

        /// <summary>
        /// Возвращает одну страницу записей, отфильтрованных по критериям.
        /// </summary>
        /// <param name="skip">Количество пропускаемых записей.</param>
        /// <param name="take">Количество записей на странице.</param>
        /// <param name="criteria">Критерии фильтрации.</param>
        /// <returns>Список Person.</returns>
        public virtual List<Person> GetPaged(int skip, int take, FilterCriteria criteria)
        {
            using (var context = new ProjectDbContext())
            {
                var query = BuildFilteredQuery(context.Persons, criteria);
                return query.OrderBy(person => person.Id).Skip(skip).Take(take).ToList();
            }
        }

        /// <summary>
        /// Возвращает общее количество записей, удовлетворяющих критериям фильтра.
        /// </summary>
        /// <param name="criteria">Критерии фильтрации.</param>
        /// <returns>Количество записей.</returns>
        public virtual int GetTotalCount(FilterCriteria criteria)
        {
            using (var context = new ProjectDbContext())
            {
                var query = BuildFilteredQuery(context.Persons, criteria);
                return query.Count();
            }
        }

        /// <summary>
        /// Возвращает все записи, удовлетворяющие критериям (без пагинации) в виде потока.
        /// Данные читаются из БД порциями, без загрузки всего набора в память.
        /// </summary>
        /// <param name="criteria">Критерии фильтрации.</param>
        /// <returns>Поток объектов Person.</returns>
        public virtual IEnumerable<Person> GetFilteredStream(FilterCriteria criteria)
        {
            using var context = new ProjectDbContext();
            var query = BuildFilteredQuery(context.Persons.AsNoTracking(), criteria);
            query = query.OrderBy(person => person.Id);
            foreach (var person in query.AsEnumerable())
            {
                yield return person;
            }
        }

        /// <summary>
        /// Применяет критерии фильтрации к запросу.
        /// </summary>
        /// <param name="query">Исходный запрос IQueryable.</param>
        /// <param name="criteria">Критерии фильтрации.</param>
        /// <returns>Модифицированный запрос.</returns>
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
        /// Добавляет одного человека в буфер. Если буфер заполнен, выполняет массовую вставку.
        /// </summary>
        /// <param name="person">Объект Person для добавления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public virtual async Task AddForBulkAsync(Person person)
        {
            _bulkBuffer.Add(person);
            if (_bulkBuffer.Count >= _batchSize)
            {
                await FlushBulkAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Принудительно вставляет все накопленные в буфере записи в базу данных.
        /// Использует EFCore.BulkExtensions с оптимизированными настройками.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public virtual async Task FlushBulkAsync()
        {
            if (_bulkBuffer.Count == 0) return;

            using (var context = new ProjectDbContext())
            {
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
        }

        /// <summary>
        /// Очищает буфер массовой вставки (например, при ошибке импорта).
        /// </summary>
        public virtual void ClearBuffer()
        {
            _bulkBuffer.Clear();
        }

        /// <summary>
        /// Асинхронно сохраняет список Person в базу данных с использованием массовой вставки (BulkInsert).
        /// </summary>
        /// <param name="persons">Список объектов Person для вставки.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public virtual async Task SaveAsync(IEnumerable<Person> persons)
        {
            using (var context = new ProjectDbContext())
            {
                var config = new BulkConfig { SetOutputIdentity = false };
                await context.BulkInsertAsync(persons.ToList(), config).ConfigureAwait(false);
            }
        }
    }
}