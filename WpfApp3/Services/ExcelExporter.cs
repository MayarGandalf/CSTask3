using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    /// <summary>
    /// Экспорт данных в файл Excel (XLSX) через OpenXmlWriter.
    /// </summary>
    public class ExcelExporter
    {
        private static readonly string[] Headers =
        {
            "Id", "Date", "FirstName", "LastName", "MiddleName", "City", "Country"
        };

        public async Task ExportAsync(IAsyncEnumerable<Person> data, string filePath)
        {
            await Task.Run(async () =>
            {
                using var spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);

                var workbookPart = spreadsheet.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                using (var writer = OpenXmlWriter.Create(worksheetPart))
                {
                    writer.WriteStartElement(new Worksheet());
                    writer.WriteStartElement(new SheetData());

                    WriteRow(writer, Headers);

                    await foreach (var person in data.ConfigureAwait(false))
                    {
                        WriteRow(writer, new[]
                        {
                            person.Id.ToString(),
                            person.Date.ToString("dd.MM.yyyy"),
                            person.FirstName ?? string.Empty,
                            person.LastName ?? string.Empty,
                            person.MiddleName ?? string.Empty,
                            person.City ?? string.Empty,
                            person.Country ?? string.Empty
                        });
                    }

                    writer.WriteEndElement(); // SheetData
                    writer.WriteEndElement(); // Worksheet
                }

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Persons"
                });

                workbookPart.Workbook.Save();
            }).ConfigureAwait(false);
        }

        private static void WriteRow(OpenXmlWriter writer, IReadOnlyList<string> values)
        {
            writer.WriteStartElement(new Row());
            for (int i = 0; i < values.Count; i++)
            {
                var cell = new Cell
                {
                    DataType = CellValues.InlineString
                };

                writer.WriteStartElement(cell);
                writer.WriteElement(new InlineString(new Text(values[i] ?? string.Empty)));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
