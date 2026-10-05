using FastEndpoints;
using LossPrevention.Application.DTO.Rules;
using LossPrevention.Application.Interfaces.Data.Rules;
using LossPrevention.Domain.Entities.Rules;
using LossPrevention.Infrastructure.Models;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Rules
{
    public class CreateRuleEndpoint : Endpoint<RuleConfigurationDTO, RuleConfigurationDTO>
    {
        private readonly MongoDbSettings _mongoDbSettings;
        private readonly IRuleConfigurationService _service;

        public CreateRuleEndpoint(MongoDbSettings mongoDbSettings, IRuleConfigurationService service)
        {
            _mongoDbSettings = mongoDbSettings;
            _service = service;
        }

        public override void Configure()
        {
            Post("/rules");
            Permissions("CAN_CREATE_RULE");
        }

        public override async Task HandleAsync(RuleConfigurationDTO req, CancellationToken ct)
        {
            if (!decimal.TryParse(req.MinValue, out var minDecimal) && !string.IsNullOrEmpty(req.MinValue))
            {
                AddError(nameof(req.MinValue), "MinValue must be a valid decimal.");
                await SendErrorsAsync(400, ct);
                return;
            }
            if (!decimal.TryParse(req.MaxValue, out var maxDecimal) && !string.IsNullOrEmpty(req.MaxValue))
            {
                AddError(nameof(req.MaxValue), "MaxValue must be a valid decimal.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var rule = new RuleConfiguration
            {
                _id = ObjectId.GenerateNewId(),
                RuleName = req.RuleName ?? string.Empty,
                RuleDescription = req.RuleDescription ?? string.Empty,
                FieldPath = req.FieldPath ?? string.Empty,
                ValueToCheck = req.ValueToCheck ?? string.Empty,
                AllowRangeCheck = req.AllowRangeCheck,
                MinValue = string.IsNullOrEmpty(req.MinValue) ? null : minDecimal,
                MaxValue = string.IsNullOrEmpty(req.MaxValue) ? null : maxDecimal,
                Enabled = req.Enabled,
                SumValues = req.SumValues
            };

            var created = await _service.CreateRuleAsync(rule, _mongoDbSettings.CollectionName_ReportData);
            await SendOkAsync(new RuleConfigurationDTO
            {
                Id = created._id.ToString(),
                RuleName = created.RuleName,
                RuleDescription = created.RuleDescription,
                FieldPath = created.FieldPath,
                ValueToCheck = created.ValueToCheck,
                AllowRangeCheck = created.AllowRangeCheck,
                MinValue = created.MinValue?.ToString(),
                MaxValue = created.MaxValue?.ToString(),
                Enabled = created.Enabled,
                SumValues = created.SumValues
            }, ct);
        }
    }
}
