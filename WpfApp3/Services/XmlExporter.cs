using System.Collections.Generic;
using System.Xml;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    /// <summary>
    /// Экспорт данных в XML-файл.
    /// </summary>
    public class XmlExporter
    {
        /// <summary>
        /// Сохраняет перечисление Person в XML-файл по указанному пути.
        /// Использует потоковую запись для работы с большими объёмами данных.
        /// </summary>
        /// <param name="data">Данные для экспорта.</param>
        /// <param name="filePath">Путь к выходному файлу.</param>
        public void Export(IEnumerable<Person> data, string filePath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };

            using var writer = XmlWriter.Create(filePath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Persons");
            writer.WriteStartElement("Records");

            foreach (var person in data)
            {
                writer.WriteStartElement("Person");
                writer.WriteElementString("Id", person.Id.ToString());
                writer.WriteElementString("Date", person.Date.ToString("yyyy-MM-dd"));
                writer.WriteElementString("FirstName", person.FirstName ?? string.Empty);
                writer.WriteElementString("LastName", person.LastName ?? string.Empty);
                writer.WriteElementString("MiddleName", person.MiddleName ?? string.Empty);
                writer.WriteElementString("City", person.City ?? string.Empty);
                writer.WriteElementString("Country", person.Country ?? string.Empty);
                writer.WriteEndElement(); // Person
            }

            writer.WriteEndElement(); // Records
            writer.WriteEndElement(); // Persons
            writer.WriteEndDocument();
            writer.Flush();
        }
    }
}