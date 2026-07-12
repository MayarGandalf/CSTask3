using System.Collections.Generic;
using System.Xml.Linq;
using System;
using WpfApp3.Models;
using System.IO;

namespace WpfApp3.Services
{
    /// <summary>
    /// Экспорт данных в XML-файл.
    /// </summary>
    public class XmlExporter
    {
        /// <summary>
        /// Сохраняет перечисление Person в XML-файл по указанному пути.
        /// </summary>
        /// <param name="data">Данные для экспорта.</param>
        /// <param name="filePath">Путь к выходному файлу.</param>
        public void Export(IEnumerable<Person> data, string filePath)
        {
            var xml = new XElement("Persons",
                new XElement("Records",
                    from person in data
                    select new XElement("Person",
                        new XElement("Id", person.Id),
                        new XElement("Date", person.Date.ToString("yyyy-MM-dd")),
                        new XElement("FirstName", person.FirstName ?? string.Empty),
                        new XElement("LastName", person.LastName ?? string.Empty),
                        new XElement("MiddleName", person.MiddleName ?? string.Empty),
                        new XElement("City", person.City ?? string.Empty),
                        new XElement("Country", person.Country ?? string.Empty)
                    )
                )
            );

            xml.Save(filePath);
        }
    }
}