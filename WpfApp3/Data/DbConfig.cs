namespace WpfApp3.Data
{
    /// <summary>
    /// Статический класс, хранящий конфигурационные параметры подключения к базе данных.
    /// </summary>
    public static class DbConfig
    {
        /// <summary>
        /// Строка подключения к экземпляру SQL Server без указания базы данных (используется для создания БД).
        /// </summary>
        public static string MasterConnectionString { get; } =
            @"Server=localhost,1433;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

        /// <summary>
        /// Имя базы данных, используемой в приложении.
        /// </summary>
        public static string DatabaseName { get; } = "CsvToDb";

        /// <summary>
        /// Полная строка подключения с указанием базы данных.
        /// </summary>
        public static string ConnectionString =>
            $"{MasterConnectionString}Database={DatabaseName};";
    }
}