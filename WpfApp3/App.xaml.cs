using System;
using System.Threading.Tasks;
using System.Windows;
using WpfApp3.Data;
using WpfApp3.Helpers;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для App.xaml.
    /// Выполняет асинхронную инициализацию базы данных при запуске приложения
    /// и показывает понятный диалог в случае ошибки подключения.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Обработчик события запуска приложения. Асинхронно инициализирует БД,
        /// обрабатывает возможные ошибки и только после успеха открывает главное окно.
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                await DBInitializer.InitializeAsync().ConfigureAwait(true);
                Logger.Info("Database initialized successfully.");

                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database initialization error");
                ShowDbErrorDialog(ex);
                Shutdown(1);
            }
        }

        private static void ShowDbErrorDialog(Exception ex)
        {
            var message =
                "Не удалось подключиться к базе данных.\n\n" +
                $"Причина: {ex.Message}\n\n" +
                "Что можно сделать:\n" +
                "  1. Убедитесь, что SQL Server запущен и доступен по адресу из config.json.\n" +
                "  2. Проверьте логин и пароль в config.json.\n" +
                "  3. Проверьте, что у пользователя есть права на создание базы данных.\n" +
                "  4. Проверьте, не блокирует ли подключение firewall.\n\n" +
                "Подробности сохранены в logs/app.log";

            MessageBox.Show(
                message,
                "Ошибка подключения к БД",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}