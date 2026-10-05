using LossPrevention.Application.Interfaces.Data;
using MongoDB.Bson;
using Microsoft.VisualBasic.FileIO;

namespace LossPrevention.Application.Services.Data
{
    public sealed class CsvProcessingService : IFileProcessingService
    {
        public string SupportedFileType => "CSV";

        public async Task<List<BsonDocument>> ProcessFileAsync(string filePath, string fileName)
        {
            var documents = new List<BsonDocument>();

            await Task.Run(() =>
            {
                using var parser = new TextFieldParser(filePath)
                {
                    TextFieldType = FieldType.Delimited,
                    Delimiters = new[] { "," },
                    HasFieldsEnclosedInQuotes = true,
                    TrimWhiteSpace = true
                };

                // Read headers
                if (parser.EndOfData)
                    return;

                var headers = parser.ReadFields();
                if (headers == null || headers.Length == 0)
                    return;

                // Read data rows
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (fields == null)
                        continue;

                    var doc = new BsonDocument();

                    for (int i = 0; i < Math.Min(headers.Length, fields.Length); i++)
                    {
                        var header = headers[i];
                        var value = fields[i];

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            // Try to parse as different types
                            if (DateTime.TryParse(value, out var dateValue))
                            {
                                doc[header] = new BsonDateTime(dateValue);
                            }
                            else if (double.TryParse(value, out var doubleValue))
                            {
                                doc[header] = doubleValue;
                            }
                            else if (bool.TryParse(value, out var boolValue))
                            {
                                doc[header] = boolValue;
                            }
                            else
                            {
                                doc[header] = value;
                            }
                        }
                    }

                    documents.Add(doc);
                }
            });

            return documents;
        }
    }
}