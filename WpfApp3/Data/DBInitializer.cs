using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WpfApp3.Helpers;

namespace WpfApp3.Data
{
    /// <summary>
    /// Предоставляет методы для инициализации базы данных: создание БД, таблиц и индексов.
    /// </summary>
    public static class DBInitializer
    {
        /// <summary>
        ///  инициализирует базу данных: создаёт БД (если отсутствует),
        /// создаёт таблицы и добавляет индексы для ускорения поиска по полям Persons.
        /// </summary>
        public static async Task InitializeAsync(CancellationToken ct = default)
        {
            await using (var connection = new SqlConnection(DbConfig.MasterConnectionString))
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                string checkDbSql =
                    $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{DbConfig.DatabaseName}') " +
                    $"CREATE DATABASE [{DbConfig.DatabaseName}]";

                await using var command = new SqlCommand(checkDbSql, connection);
                await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await using var context = new ProjectDbContext();
            await context.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);

            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Persons_LastName' AND object_id = OBJECT_ID('Persons'))
                CREATE INDEX IX_Persons_LastName ON Persons(LastName);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Persons_FirstName' AND object_id = OBJECT_ID('Persons'))
                CREATE INDEX IX_Persons_FirstName ON Persons(FirstName);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Persons_MiddleName' AND object_id = OBJECT_ID('Persons'))
                CREATE INDEX IX_Persons_MiddleName ON Persons(MiddleName);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Persons_City' AND object_id = OBJECT_ID('Persons'))
                CREATE INDEX IX_Persons_City ON Persons(City);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Persons_Country' AND object_id = OBJECT_ID('Persons'))
                CREATE INDEX IX_Persons_Country ON Persons(Country);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Persons_Date' AND object_id = OBJECT_ID('Persons'))
                CREATE INDEX IX_Persons_Date ON Persons(Date);
            ", ct).ConfigureAwait(false);
        }
    }
}
