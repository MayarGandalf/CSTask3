using System;
using System.Windows.Input;

namespace WpfApp3.Commands
{
    /// <summary>
    /// Базовая реализация команды <see cref="ICommand"/> без параметров.
    /// Используется для привязки действий в ViewModel к элементам управления.
    /// </summary>
    public class CommandBase : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Инициализирует новый экземпляр команды.
        /// </summary>
        /// <param name="execute">Делегат, выполняющий основное действие команды.</param>
        /// <param name="canExecute">Делегат, определяющий доступность команды (необязательный).</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="execute"/> равен null.</exception>
        public CommandBase(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие, возникающее при изменении состояния доступности команды.
        /// Подписывается на глобальное событие <see cref="CommandManager.RequerySuggested"/>.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Определяет, может ли команда выполняться в текущий момент.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        /// <returns>true, если команда доступна; иначе false.</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Выполняет основное действие команды.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        public void Execute(object? parameter) => _execute();
    }

    /// <summary>
    /// Обобщённая реализация команды <see cref="ICommand"/> с параметром типа <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Тип параметра, передаваемого в команду.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// Инициализирует новый экземпляр параметризованной команды.
        /// </summary>
        /// <param name="execute">Делегат, выполняющий основное действие команды с параметром.</param>
        /// <param name="canExecute">Делегат, определяющий доступность команды на основе переданного параметра (необязательный).</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="execute"/> равен null.</exception>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие, возникающее при изменении состояния доступности команды.
        /// Подписывается на глобальное событие <see cref="CommandManager.RequerySuggested"/>.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Определяет, может ли команда выполняться с переданным параметром.
        /// </summary>
        /// <param name="parameter">Параметр команды (тип <typeparamref name="T"/>).</param>
        /// <returns>true, если команда доступна; иначе false.</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        /// <summary>
        /// Выполняет основное действие команды с переданным параметром.
        /// </summary>
        /// <param name="parameter">Параметр команды (тип <typeparamref name="T"/>).</param>
        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}