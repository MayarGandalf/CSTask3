using System;

namespace WpfApp3.Services
{
    /// <summary>
    /// Класс-контейнер для критериев фильтрации записей Person.
    /// </summary>
    public class FilterCriteria
    {
        /// <summary>Фильтр по идентификатору (точное совпадение).</summary>
        public int? Id { get; set; }

        /// <summary>Фильтр по имени (содержит подстроку).</summary>
        public string? FirstName { get; set; }

        /// <summary>Фильтр по фамилии (содержит подстроку).</summary>
        public string? LastName { get; set; }

        /// <summary>Фильтр по отчеству (содержит подстроку).</summary>
        public string? MiddleName { get; set; }

        /// <summary>Фильтр по городу (содержит подстроку).</summary>
        public string? City { get; set; }

        /// <summary>Фильтр по стране (содержит подстроку).</summary>
        public string? Country { get; set; }

        /// <summary>Начальная дата для фильтрации (включительно).</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>Конечная дата для фильтрации (включительно).</summary>
        public DateTime? DateTo { get; set; }
    }
}