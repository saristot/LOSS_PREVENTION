using FastEndpoints;
using LossPrevention.Application.Interfaces.Data.Rules;

namespace LossPrevention.API.Endpoints.Rules
{

public class ApplyRulesEndpoint : EndpointWithoutRequest
{
    private readonly IRuleConfigurationService _service;

    public ApplyRulesEndpoint(IRuleConfigurationService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/rules/apply");
        Permissions("CAN_APPLY_RULE");
        Summary(s => s.Summary = "Evaluate a document using one or more rules.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.ApplyRulesAsync();
        await SendOkAsync(result, ct);
    }
}

}
