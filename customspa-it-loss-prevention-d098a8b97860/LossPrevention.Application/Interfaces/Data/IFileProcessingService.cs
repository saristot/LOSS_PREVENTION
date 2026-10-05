using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.Data
{
    public interface IFileProcessingService
    {
        /// <summary>
        /// Process a file and return the documents to be inserted
        /// </summary>
        Task<List<BsonDocument>> ProcessFileAsync(string filePath, string fileName);

        /// <summary>
        /// The file type this processor handles (e.g., "XML", "CSV", "JSON")
        /// </summary>
        string SupportedFileType { get; }
    }
}