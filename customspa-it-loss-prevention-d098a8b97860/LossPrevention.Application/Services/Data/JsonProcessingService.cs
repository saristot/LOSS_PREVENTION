using LossPrevention.Application.Interfaces.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Text.Json;

namespace LossPrevention.Application.Services.Data
{
    public sealed class JsonProcessingService : IFileProcessingService
    {
        public string SupportedFileType => "JSON";

        public async Task<List<BsonDocument>> ProcessFileAsync(string filePath, string fileName)
        {
            var documents = new List<BsonDocument>();
            var jsonContent = await File.ReadAllTextAsync(filePath);

            try
            {
                // Try to parse as JSON array first
                using var jsonDoc = JsonDocument.Parse(jsonContent);

                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    // Multiple documents in array
                    foreach (var element in jsonDoc.RootElement.EnumerateArray())
                    {
                        var bsonDoc = BsonSerializer.Deserialize<BsonDocument>(element.GetRawText());
                        documents.Add(bsonDoc);
                    }
                }
                else
                {
                    // Single document
                    var bsonDoc = BsonSerializer.Deserialize<BsonDocument>(jsonContent);
                    documents.Add(bsonDoc);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse JSON file {fileName}: {ex.Message}", ex);
            }

            return documents;
        }
    }
}