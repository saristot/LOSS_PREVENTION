using FastEndpoints;
using LossPrevention.API.Handlers.Requests.Rules;
using LossPrevention.Application.Interfaces.Data.Rules;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Rules
{
    public class DeleteRuleEndpoint : Endpoint<IdRequest, bool>
    {
        private readonly IRuleConfigurationService _service;

        public DeleteRuleEndpoint(IRuleConfigurationService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Delete("/rules/{id}");
            Permissions("CAN_DELETE_RULE");
        }

        public override async Task HandleAsync(IdRequest req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.Id, out var objectId))
            {
                AddError(nameof(req.Id), "Id must be a valid ObjectId.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var deleted = await _service.DeleteRuleAsync(objectId);
            if (!deleted)
            {
                await SendNotFoundAsync(ct);
                return;
            }
            await SendNoContentAsync(ct);
        }
    }
}
