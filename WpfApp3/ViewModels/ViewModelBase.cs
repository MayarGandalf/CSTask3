using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp3.ViewModels
{
    /// <summary>
    /// Базовый абстрактный класс для моделей представления (ViewModel),
    /// реализующий интерфейс <see cref="INotifyPropertyChanged"/>.
    /// Предоставляет методы для уведомления об изменениях свойств.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие, возникающее при изменении значения свойства.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Вызывает событие <see cref="PropertyChanged"/> для указанного свойства.
        /// </summary>
        /// <param name="propertyName">Имя свойства, которое изменилось. Если не указано, используется имя вызывающего члена.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение поля и вызывает событие <see cref="PropertyChanged"/>,
        /// если новое значение отличается от текущего.
        /// </summary>
        /// <typeparam name="T">Тип значения свойства.</typeparam>
        /// <param name="field">Ссылка на поле, хранящее значение свойства.</param>
        /// <param name="value">Новое значение.</param>
        /// <param name="propertyName">Имя свойства (автоматически определяется вызывающим членом).</param>
        /// <returns>true, если значение было изменено; иначе false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}