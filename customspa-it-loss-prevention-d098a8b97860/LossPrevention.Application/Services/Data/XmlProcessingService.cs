using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Application.Helpers;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using System.Xml.Linq;

namespace LossPrevention.Application.Services.Data
{
    public sealed class XmlProcessingService : IFileProcessingService, IXmlProcessingService
    {
        private readonly IXmlEnrichmentService _enrichmentService;

        public string SupportedFileType => "XML";

        public XmlProcessingService(IXmlEnrichmentService enrichmentService)
        {
            _enrichmentService = enrichmentService;
        }

        // New method for IFileProcessingService
        public async Task<List<BsonDocument>> ProcessFileAsync(string filePath, string fileName)
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            var xml = XElement.Parse(xmlContent);
            var enrichedXml = _enrichmentService.Enrich(xml);
            var bson = XmlToBsonConverterHelper.ConvertFlattened(enrichedXml.ToString());

            return new List<BsonDocument> { bson };
        }

        // Keep old methods for backward compatibility
        public async Task<BsonDocument> ProcessAsync(string xmlString)
        {
            var xml = XElement.Parse(xmlString);
            var enrichedXml = _enrichmentService.Enrich(xml);
            var bson = XmlToBsonConverterHelper.ConvertFlattened(enrichedXml.ToString());
            return bson;
        }

        public Task InsertManyAsync(IEnumerable<BsonDocument> docs)
        {
            // This method is deprecated - FileProcessingCoordinator handles insertion now
            throw new NotImplementedException("Use FileProcessingCoordinator.RunIngestionAsync instead");
        }
    }
}