using FastEndpoints;
using LossPrevention.Application.Interfaces.Data;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Mappings
{

    public class DeleteMappingEndpoint : EndpointWithoutRequest
    {
        private readonly IMappingService _mappingService;

        public DeleteMappingEndpoint(IMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public override void Configure()
        {
            Delete("/data/mappings/{id}");
            Permissions("CAN_DELETE_MAPPINGS");

            Summary(s =>
            {
                s.Summary = "Delete a mapping by its ID.";
                s.Responses[200] = "Mapping deleted successfully.";
                s.Responses[400] = "Invalid ID format.";
                s.Responses[404] = "Mapping not found.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<string>("id");

            if (!ObjectId.TryParse(id, out var objectId))
            {
                AddError("Invalid ID format.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var result = await _mappingService.DeleteMappingAsync(objectId);

            if (result.DeletedCount == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(ct);
        }
    }

}
