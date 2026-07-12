using System.Windows.Controls;
using System.Windows;

namespace WpfApp3.Services
{
    /// <summary>
    /// Сервис для работы с элементами управления фильтрации: сбор критериев и сброс полей.
    /// </summary>
    public class FilterService
    {
        /// <summary>
        /// Собирает критерии фильтрации из текстовых полей и DatePicker.
        /// </summary>
        /// <param name="tbId">Поле для ввода Id.</param>
        /// <param name="tbFirstName">Поле для ввода имени.</param>
        /// <param name="tbLastName">Поле для ввода фамилии.</param>
        /// <param name="tbMiddleName">Поле для ввода отчества.</param>
        /// <param name="tbCity">Поле для ввода города.</param>
        /// <param name="tbCountry">Поле для ввода страны.</param>
        /// <param name="dpDateFrom">Выбор начальной даты.</param>
        /// <param name="dpDateTo">Выбор конечной даты.</param>
        /// <returns>Объект FilterCriteria с заполненными полями.</returns>
        public FilterCriteria GetCriteria(
            TextBox tbId,
            TextBox tbFirstName,
            TextBox tbLastName,
            TextBox tbMiddleName,
            TextBox tbCity,
            TextBox tbCountry,
            DatePicker dpDateFrom,
            DatePicker dpDateTo)
        {
            return new FilterCriteria
            {
                Id = int.TryParse(tbId.Text, out int id) ? id : (int?)null,
                FirstName = tbFirstName.Text,
                LastName = tbLastName.Text,
                MiddleName = tbMiddleName.Text,
                City = tbCity.Text,
                Country = tbCountry.Text,
                DateFrom = dpDateFrom.SelectedDate,
                DateTo = dpDateTo.SelectedDate
            };
        }

        /// <summary>
        /// Очищает все поля фильтрации, устанавливая их в исходное состояние.
        /// </summary>
        /// <param name="tbId">Поле Id.</param>
        /// <param name="tbFirstName">Поле имени.</param>
        /// <param name="tbLastName">Поле фамилии.</param>
        /// <param name="tbMiddleName">Поле отчества.</param>
        /// <param name="tbCity">Поле города.</param>
        /// <param name="tbCountry">Поле страны.</param>
        /// <param name="dpDateFrom">Выбор начальной даты.</param>
        /// <param name="dpDateTo">Выбор конечной даты.</param>
        public void ClearFilters(
            TextBox tbId,
            TextBox tbFirstName,
            TextBox tbLastName,
            TextBox tbMiddleName,
            TextBox tbCity,
            TextBox tbCountry,
            DatePicker dpDateFrom,
            DatePicker dpDateTo)
        {
            tbId.Clear();
            tbFirstName.Clear();
            tbLastName.Clear();
            tbMiddleName.Clear();
            tbCity.Clear();
            tbCountry.Clear();
            dpDateFrom.SelectedDate = null;
            dpDateTo.SelectedDate = null;
        }
    }
}