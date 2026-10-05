using LossPrevention.Application.Interfaces.Indexes;
using LossPrevention.Domain.Entities.Data;
using LossPrevention.Domain.Entities.Indexes;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.Data
{
    public sealed class IndexService : IIndexService
    {
        private readonly IMongoRepository<MappingItem> _mappingRepository;
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;

        public IndexService(IMongoRepository<MappingItem> mappingRepository, IMongoRepository<BsonDocument> reportDataRepository)
        {
            _mappingRepository = mappingRepository;
            _reportDataRepository = reportDataRepository;
        }

        public async Task ProcessIndexesAsync()
        {
            var documents = await _reportDataRepository.FindManyAsync(_ => true, 0, 100);
            var suggestions = new HashSet<IndexSuggestion>();

            foreach (var document in documents)
            {
                var indexSuggestions = SuggestIndexes(document);

                foreach (var suggestion in indexSuggestions)
                {
                    if (!suggestions.Any(s => s.FieldName == suggestion.FieldName))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }
            await _reportDataRepository.CreateIndexesAsync(suggestions.Select(x => x.FieldName));
        }


        private HashSet<IndexSuggestion> SuggestIndexes(BsonDocument document)
        {
            var suggestions = new HashSet<IndexSuggestion>();

            foreach (var element in document.Elements)
            {
                var name = element.Name;
                var value = element.Value;

                if (name == "_id")
                    continue; // Already indexed by default

                if (value.IsBoolean)
                {
                    suggestions.Add(new IndexSuggestion
                    {
                        FieldName = name,
                        Reason = "Boolean field - consider indexing if it's used for filtering and is selective."
                    });
                }
                else if (value.IsBsonDateTime)
                {
                    suggestions.Add(new IndexSuggestion
                    {
                        FieldName = name,
                        Reason = "DateTime field - consider indexing for range or sort queries."
                    });
                }
                else if (value.IsBsonArray)
                {
                    suggestions.Add(new IndexSuggestion
                    {
                        FieldName = name,
                        Reason = "Array field - consider multikey index if used in queries."
                    });
                }
                else if (value.IsString || value.IsInt32 || value.IsInt64 || value.IsDecimal128)
                {
                    suggestions.Add(new IndexSuggestion
                    {
                        FieldName = name,
                        Reason = "Likely to be used in equality or filtering queries."
                    });
                }
            }

            return suggestions;
        }
    }
}
