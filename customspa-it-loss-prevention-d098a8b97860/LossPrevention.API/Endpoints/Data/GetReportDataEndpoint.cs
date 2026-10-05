using FastEndpoints;
using Handlers.Requests.Data;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.API.Handlers.Responses;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LossPrevention.API.Handlers.Endpoints.Data;

public class GetReportDataEndpoint : Endpoint<GetReportDataRequest, GetReportDataResponse>
{
    private readonly IReportDataservice _reportDataService;
    private readonly IMappingService _mappingService;
    private readonly IMemoryCache _cache;

    public GetReportDataEndpoint(IReportDataservice reportDataService, IMappingService mappingService, IMemoryCache cache)
    {
        _reportDataService = reportDataService;
        _mappingService = mappingService;
        _cache = cache;
    }

    public override void Configure()
    {
        Post("/data/report/query");
        Permissions("CAN_VIEW_REPORT");
        Description(b =>
        {
            b.WithName("QueryReportData");
            b.Produces<List<Dictionary<string, object>>>(200);
            b.ProducesProblem(400);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary
        {
            Description = "Returns a flattened list of report data from the report collection using an aggregation pipeline."
        });
    }

    public override async Task HandleAsync(GetReportDataRequest req, CancellationToken ct)
    {
        if (req.Take <= 0 || req.QueryPipeline is null || req.QueryPipeline.Count == 0)
        {
            AddError("query", "Missing or invalid 'Take' or 'QueryPipeline'.");
            await SendErrorsAsync(400, ct);
            return;
        }

        try
        {
            ////// Generate cache key using hash of the serialized request
            var cacheKey = GenerateCacheKey(req);

            //// Try getting from cache
            if (_cache.TryGetValue(cacheKey, out GetReportDataResponse cachedResponse))
            {
                await SendOkAsync(cachedResponse, ct);
                return;
            }

            var bsonList = await _mappingService.GetAllMappings(); // Retained for any side-effects/mapping use

            var pipeline = req.QueryPipeline
                              .Where(x => x != null)
                              .Select(x => BsonDocument.Parse(x.ToString()))
                              .ToArray();

            var result = await _reportDataService.QueryReportDataAsync(pipeline, req.Skip, req.Take);
            var filteredData = result.Data.Where(dict => dict.Count > 0).ToList();

            var response = new GetReportDataResponse
            {
                Data = filteredData,
                Total = result.TotalCount
            };

            // Cache the result
            _cache.Set(cacheKey, response, TimeSpan.FromHours(1));

            await SendOkAsync(response, ct);
        }
        catch (Exception ex)
        {
            AddError("query", "Invalid query pipeline: " + ex.Message);
            await SendErrorsAsync(400, ct);
        }
    }

    private string GenerateCacheKey(GetReportDataRequest req)
    {
        var json = JsonSerializer.Serialize(new { req.QueryPipeline, req.Skip, req.Take });
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return $"report-query:{Convert.ToBase64String(hashBytes)}";
    }
}
