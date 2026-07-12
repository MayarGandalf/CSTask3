using System;

namespace WpfApp3.Services
{
    /// <summary>
    /// Сервис для управления пагинацией данных: текущая страница, размер страницы, общее количество записей.
    /// </summary>
    public class PaginationService
    {
        private int _currentPage = 1;
        private int _totalRecords = 0;
        private int _pageSize;

        /// <summary>
        /// Инициализирует сервис пагинации с указанным размером страницы.
        /// </summary>
        /// <param name="pageSize">Количество записей на странице (по умолчанию 100).</param>
        public PaginationService(int pageSize = 100)
        {
            _pageSize = pageSize;
        }

        /// <summary>Номер текущей страницы (начиная с 1).</summary>
        public int CurrentPage => _currentPage;

        /// <summary>Размер страницы.</summary>
        public int PageSize => _pageSize;

        /// <summary>Общее количество записей.</summary>
        public int TotalRecords => _totalRecords;

        /// <summary>Общее количество страниц.</summary>
        public int TotalPages => (int)Math.Ceiling((double)_totalRecords / _pageSize);

        /// <summary>Количество записей, которые нужно пропустить (для SQL OFFSET).</summary>
        public int Skip => (_currentPage - 1) * _pageSize;

        /// <summary>Устанавливает общее количество записей.</summary>
        /// <param name="total">Общее количество.</param>
        public void SetTotalRecords(int total) => _totalRecords = total;

        /// <summary>Устанавливает текущую страницу с проверкой границ.</summary>
        /// <param name="page">Номер страницы (будет скорректирован в допустимый диапазон).</param>
        public void SetCurrentPage(int page) => _currentPage = Math.Max(1, Math.Min(page, TotalPages));

        /// <summary>Переход на следующую страницу, если возможно.</summary>
        public void NextPage() { if (_currentPage < TotalPages) _currentPage++; }

        /// <summary>Переход на предыдущую страницу, если возможно.</summary>
        public void PrevPage() { if (_currentPage > 1) _currentPage--; }

        /// <summary>Сброс на первую страницу.</summary>
        public void Reset() => _currentPage = 1;

        /// <summary>Можно ли перейти на предыдущую страницу.</summary>
        public bool CanGoPrevious => _currentPage > 1;

        /// <summary>Можно ли перейти на следующую страницу.</summary>
        public bool CanGoNext => _currentPage < TotalPages;
    }
}