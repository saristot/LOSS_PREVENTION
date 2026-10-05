using FastEndpoints;
using LossPrevention.Application.DTO.Mappings;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Domain.Entities.Data;
using MongoDB.Bson;

namespace Endpoints.Mappings;

public class GetMappingsEndpoint : EndpointWithoutRequest<List<MappingItemDTO>>
{
    private readonly IMappingService _mappingService;

    public GetMappingsEndpoint(IMappingService mappingService)
    {
        _mappingService = mappingService;
    }

    public override void Configure()
    {
        Get("/data/mappings");

        Permissions("CAN_VIEW_MAPPINGS");
        Description(b =>
        {
            b.WithName("GetMappings");
            b.Produces(200);
            b.ProducesProblem(400);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary
        {
            Description = "Returns a list of all field mappings."
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var mappings = await _mappingService.GetAllMappings();
        var dtoList = mappings.Select(m => new MappingItemDTO
        {
            Id = m._id.ToString(),
            Name = m.Name,
            Alias = m.Alias,
            DataType = m.DataType,
            IsVisible = m.IsVisible,
            IsArray = m.IsArray,
            IsCalculated = m.IsCalculated,
            IsLookup = m.IsLookup,
            CollectionName = m.CollectionName,
            LongestLength = m.LongestLength
        }).ToList();

        // If you want to include the id as a string, add a property to MappingItemDTO (e.g., Id)
        // and set: Id = m._id.ToString()

        await SendOkAsync(dtoList, ct);
    }

}
