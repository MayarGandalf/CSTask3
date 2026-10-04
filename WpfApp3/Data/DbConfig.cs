using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WpfApp3.Data
{
    /// <summary>
    /// Статический класс, читающий параметры подключения к БД из config.json
    /// и формирующий строки подключения.
    /// </summary>
    public static class DbConfig
    {
        private static readonly IConfigurationRoot _config;

        static DbConfig()
        {
            _config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("config.json", optional: false, reloadOnChange: false).Build();
        }

        public static string Server => _config["Database:Server"] ?? throw new InvalidOperationException("Не задан параметр Database:Server в config.json");

        public static string DatabaseName => _config["Database:Database"] ?? throw new InvalidOperationException("Не задан параметр Database:Database в config.json");

        public static string UserId => _config["Database:UserId"] ?? throw new InvalidOperationException("Не задан параметр Database:UserId в config.json");

        public static string Password => _config["Database:Password"] ?? throw new InvalidOperationException("Не задан параметр Database:Password в config.json");

        public static bool TrustServerCertificate => bool.TryParse(_config["Database:TrustServerCertificate"], out var v) && v;

        public static int ConnectTimeout => int.TryParse(_config["Database:ConnectTimeout"], out var v) && v > 0 ? v : 15;

        private static string BuildConnectionString(string? database)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                UserID = UserId,
                Password = Password,
                TrustServerCertificate = TrustServerCertificate,
                ConnectTimeout = ConnectTimeout
            };

            if (!string.IsNullOrEmpty(database))
                builder.InitialCatalog = database;

            return builder.ConnectionString;
        }

        /// <summary>
        /// Строка подключения к экземпляру SQL Server без указания базы данных
        /// (используется для создания БД).
        /// </summary>
        public static string MasterConnectionString => BuildConnectionString(null);

        /// <summary>
        /// Полная строка подключения с указанием базы данных.
        /// </summary>
        public static string ConnectionString => BuildConnectionString(DatabaseName);
    }
}