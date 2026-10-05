using FastEndpoints;
using LossPrevention.Application.DTO.Mappings;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Domain.Entities.Data;

namespace LossPrevention.API.Endpoints.Mappings
{
    public class UpdateMappingEndpoint : Endpoint<MappingItemDTO>
    {
        private readonly IMappingService _mappingService;

        public UpdateMappingEndpoint(IMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public override void Configure()
        {
            Put("/data/mappings/{id}");
            Permissions("CAN_UPDATE_MAPPINGS");

            Summary(s =>
            {
                s.Summary = "Update an existing field mapping.";
                s.Responses[200] = "Mapping updated successfully.";
                s.Responses[400] = "Invalid input or ID.";
                s.Responses[404] = "Mapping not found.";
            });
        }

        public override async Task HandleAsync(MappingItemDTO req, CancellationToken ct)
        {
            var id = Route<string>("id");

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
                await _mappingService.UpdateMappingAsync(id, mapping);
                await SendOkAsync(cancellation: ct);
            }
            catch (KeyNotFoundException ex)
            {
                await SendNotFoundAsync(ct);
            }
            catch (Exception ex)
            {
                AddError(ex.Message);
                await SendErrorsAsync(cancellation: ct);
            }
        }
    }

}
