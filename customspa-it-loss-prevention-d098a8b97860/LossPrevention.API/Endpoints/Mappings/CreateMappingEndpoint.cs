using FastEndpoints;
using LossPrevention.Application.DTO.Mappings;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Domain.Entities.Data;

namespace LossPrevention.API.Endpoints.Mappings
{
    public class CreateMappingEndpoint : Endpoint<MappingItemDTO>
    {
        private readonly IMappingService _mappingService;

        public CreateMappingEndpoint(IMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public override void Configure()
        {
            Post("/data/mappings");
            Permissions("CAN_CREATE_MAPPINGS");

            Summary(s =>
            {
                s.Summary = "Create a new field mapping.";
                s.Responses[201] = "Mapping created successfully.";
                s.Responses[400] = "Invalid input or mapping already exists.";
            });
        }

        public override async Task HandleAsync(MappingItemDTO req, CancellationToken ct)
        {
            var mapping = new MappingItem
            {
                Name = req.Name,
                Alias = req.Alias,
                DataType = req.DataType,
                IsVisible = req.IsVisible,
                IsArray = req.IsArray,
                IsCalculated = req.IsCalculated,
                IsLookup = req.IsLookup,
                CollectionName = req.CollectionName,
                LongestLength = req.LongestLength
            };

            try
            {
                await _mappingService.AddMappingAsync(mapping);
                await SendCreatedAtAsync("GetMappings", new { mapping.Name }, mapping, cancellation: ct);
            }
            catch (Exception ex)
            {
                AddError(ex.Message);
                await SendErrorsAsync(cancellation: ct);
            }
        }
    }
}
