using FastEndpoints;
using LossPrevention.API.Handlers.Requests.Rules;
using LossPrevention.Application.DTO.Rules;
using LossPrevention.Application.Interfaces.Data.Rules;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Rules
{
    public class GetRuleByIdEndpoint : Endpoint<IdRequest, RuleConfigurationDTO>
    {
        private readonly IRuleConfigurationService _service;

        public GetRuleByIdEndpoint(IRuleConfigurationService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/rules/{id}");
            Permissions("CAN_VIEW_RULE");
        }

        public override async Task HandleAsync(IdRequest req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.Id, out var objectId))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var rule = await _service.GetRuleByIdAsync(objectId);
            if (rule is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(new RuleConfigurationDTO
            {
                Id = rule._id.ToString(),
                RuleName = rule.RuleName,
                RuleDescription = rule.RuleDescription,
                FieldPath = rule.FieldPath,
                ValueToCheck = rule.ValueToCheck,
                AllowRangeCheck = rule.AllowRangeCheck,
                MinValue = rule.MinValue?.ToString(),
                MaxValue = rule.MaxValue?.ToString(),
                Enabled = rule.Enabled,
                SumValues = rule.SumValues
            }, ct);
        }
    }
}
