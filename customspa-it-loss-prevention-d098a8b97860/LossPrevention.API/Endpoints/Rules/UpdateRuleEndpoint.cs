using FastEndpoints;
using LossPrevention.Application.DTO.Rules;
using LossPrevention.Application.Interfaces.Data.Rules;
using LossPrevention.Domain.Entities.Rules;
using LossPrevention.Infrastructure.Models;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Rules
{
    public class UpdateRuleEndpoint : Endpoint<RuleConfigurationDTO, bool>
    {
        private readonly MongoDbSettings _mongoDbSettings;
        private readonly IRuleConfigurationService _service;

        public UpdateRuleEndpoint(MongoDbSettings mongoDbSettings, IRuleConfigurationService service)
        {
            _mongoDbSettings = mongoDbSettings;
            _service = service;
        }

        public override void Configure()
        {
            Put("/rules");
            Permissions("CAN_UPDATE_RULE");
        }

        public override async Task HandleAsync(RuleConfigurationDTO req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.Id, out var objectId))
            {
                AddError(nameof(req.Id), "Id must be a valid ObjectId.");
                await SendErrorsAsync(400, ct);
                return;
            }
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
                _id = objectId,
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

            var success = await _service.UpdateRuleAsync(rule, _mongoDbSettings.CollectionName_ReportData);
            if (!success)
            {
                await SendNotFoundAsync(ct);
                return;
            }
            await SendOkAsync(ct);
        }
    }
}
