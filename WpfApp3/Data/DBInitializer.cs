using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace WpfApp3.Data
{
    /// <summary>
    /// Предоставляет методы для инициализации базы данных: создание БД, таблиц и индексов.
    /// </summary>
    public static class DBInitializer
    {
        /// <summary>
        /// Выполняет инициализацию базы данных: создаёт базу данных (если отсутствует),
        /// создаёт таблицы и добавляет индексы для ускорения поиска по полям Persons.
        /// </summary>
        public static void Initialize()
        {
            using (var connection = new SqlConnection(DbConfig.MasterConnectionString))
            {
                connection.Open();
                string checkDbSql = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{DbConfig.DatabaseName}') " +
                                    $"CREATE DATABASE [{DbConfig.DatabaseName}]";
                using (var command = new SqlCommand(checkDbSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            using (var context = new DbContext())
            {
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlRaw(@"
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
                ");
            }
        }
    }
}