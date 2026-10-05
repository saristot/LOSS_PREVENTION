using FastEndpoints;
using LossPrevention.Application.DTO.Rules;
using LossPrevention.Application.Interfaces.Data.Rules;

namespace LossPrevention.API.Endpoints.Rules
{
    public class GetAllRulesEndpoint : EndpointWithoutRequest<List<RuleConfigurationDTO>>
    {
        private readonly IRuleConfigurationService _service;

        public GetAllRulesEndpoint(IRuleConfigurationService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/rules");
            Permissions("CAN_VIEW_RULE");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await _service.GetAllRulesAsync();
            var dtoResult = result.Select(rule => new RuleConfigurationDTO
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
            }).ToList();
            await SendOkAsync(dtoResult, ct);
        }
    }
}
