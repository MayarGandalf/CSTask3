using System;
using System.Windows;
using WpfApp3.Data;
using WpfApp3.Helpers; // добавлено

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для App.xaml.
    /// Выполняет инициализацию базы данных при запуске приложения.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Обработчик события запуска приложения. Инициализирует БД и обрабатывает возможные ошибки.
        /// </summary>
        /// <param name="e">Аргументы запуска.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                DBInitializer.Initialize();
                Logger.Info("База данных инициализирована успешно."); // добавлено
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка инициализации БД"); // добавлено
                MessageBox.Show($"Ошибка инициализации БД:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}