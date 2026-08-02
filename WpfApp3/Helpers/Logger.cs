using System;
using System.IO;

namespace WpfApp3.Helpers
{
    /// <summary>
    /// Простой статический логгер для записи сообщений в файл.
    /// </summary>
    public static class Logger
    {
        // Путь к файлу лога 
        private static readonly string LogFilePath = "logs/app.log";

        static Logger()
        {
            // Создаём папку для логов, если её нет
            var directory = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// Записывает сообщение в лог с указанным уровнем.
        /// </summary>
        private static void WriteLog(string message, string level = "INFO")
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Если запись в файл невозможна – игнорируем, чтобы не нарушить работу приложения
            }
        }

        public static void Info(string message) => WriteLog(message, "INFO");
        public static void Warning(string message) => WriteLog(message, "WARN");
        public static void Error(string message) => WriteLog(message, "ERROR");

        /// <summary>
        /// Логирует исключение с сообщением и стек-трейсом.
        /// </summary>
        public static void Error(Exception ex, string? message = null)
        {
            string fullMessage = string.IsNullOrEmpty(message) ? $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : $"{message} - {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            WriteLog(fullMessage, "ERROR");
        }
    }
}