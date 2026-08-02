using Microsoft.EntityFrameworkCore;
using WpfApp3.Models;

namespace WpfApp3.Data
{
    /// <summary>
    /// Контекст базы данных Entity Framework Core для работы с сущностью Person.
    /// </summary>
    public class ProjectDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        /// <summary>
        /// Набор сущностей Person, представляющий таблицу Persons.
        /// </summary>
        public DbSet<Person> Persons { get; set; }

        /// <summary>
        /// Настраивает параметры подключения к базе данных.
        /// </summary>
        /// <param name="optionsBuilder">Построитель параметров контекста.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DbConfig.ConnectionString);
        }
    }
}