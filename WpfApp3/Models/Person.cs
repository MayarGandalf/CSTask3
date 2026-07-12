using System;
using System.ComponentModel.DataAnnotations;

namespace WpfApp3.Models
{
    /// <summary>
    /// Представляет запись о человеке, загруженную из CSV-файла.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Уникальный идентификатор записи (автоинкремент).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата, связанная с записью.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Имя человека. Максимальная длина – 100 символов.
        /// </summary>
        [MaxLength(100)]
        public string? FirstName { get; set; }

        /// <summary>
        /// Фамилия человека. Максимальная длина – 100 символов.
        /// </summary>
        [MaxLength(100)]
        public string? LastName { get; set; }

        /// <summary>
        /// Отчество человека. Максимальная длина – 100 символов.
        /// </summary>
        [MaxLength(100)]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Город. Максимальная длина – 100 символов.
        /// </summary>
        [MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Страна. Максимальная длина – 100 символов.
        /// </summary>
        [MaxLength(100)]
        public string? Country { get; set; }
    }
}