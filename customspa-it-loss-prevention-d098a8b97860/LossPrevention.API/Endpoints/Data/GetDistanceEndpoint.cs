using FastEndpoints;
using LossPrevention.API.Handlers.Requests.Data;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Application.Services.Data;
using Microsoft.Extensions.Caching.Memory;

public class GetDistanceEndpoint : Endpoint<DistanceRequest>
{
    private readonly IDistanceDataservice _distanceDataService;
    private readonly IMappingService _mappingService;
    private readonly IMemoryCache _cache;

    public GetDistanceEndpoint(IDistanceDataservice distanceDataService, IMappingService mappingService, IMemoryCache cache)
    {
        _distanceDataService = distanceDataService;
        _mappingService = mappingService;
        _cache = cache;
    }

    public override void Configure()
    {
        Permissions("CAN_VIEW_REPORT");
        Post("/distance");

        Summary(s =>
        {
            s.Summary = "Computes Euclidean Distance for provided fields and date range.";
        });
    }

    public override async Task HandleAsync(DistanceRequest req, CancellationToken ct)
    {
        // You can extract field names from the input fields
        //var comparisonFields = req.Fields.Select(f => f.Name).ToList();

        // Assuming your GetDistanceAsync is updated to accept these parameters
        var result = await _distanceDataService.GetDistanceAsync(
            sourceDocumentId: req.Id, // "6831e59ccc5492cce99141aa"
            startDateField: req.StartDateField,
            endDateField: req.EndDateField,
            startDate: req.StartDate,
            endDate: req.EndDate,
            keyField : req.KeyField,
            comparisonFields: req.Fields
        );

        await SendOkAsync(result, ct);
    }
}
