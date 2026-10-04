using System.Threading.Tasks;
using System.Xml;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    /// <summary>
    /// Экспорт данных в XML-файл. Работает потоково через XmlWriter.
    /// </summary>
    public class XmlExporter
    {
        public async Task ExportAsync(IAsyncEnumerable<Person> data, string filePath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Async = true
            };

            await using var writer = XmlWriter.Create(filePath, settings);
            await writer.WriteStartDocumentAsync().ConfigureAwait(false);
            await writer.WriteStartElementAsync(null, "Persons", null).ConfigureAwait(false);
            await writer.WriteStartElementAsync(null, "Records", null).ConfigureAwait(false);

            await foreach (var person in data.ConfigureAwait(false))
            {
                await writer.WriteStartElementAsync(null, "Person", null).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "Id", null, person.Id.ToString()).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "Date", null, person.Date.ToString("yyyy-MM-dd")).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "FirstName", null, person.FirstName ?? string.Empty).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "LastName", null, person.LastName ?? string.Empty).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "MiddleName", null, person.MiddleName ?? string.Empty).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "City", null, person.City ?? string.Empty).ConfigureAwait(false);
                await writer.WriteElementStringAsync(null, "Country", null, person.Country ?? string.Empty).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndDocumentAsync().ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}