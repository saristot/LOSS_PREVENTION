using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Data;
using LossPrevention.Application.Interfaces.Data;

namespace LossPrevention.API.Handlers.Requests.Data;

public class CreateTransactionEndpoint : Endpoint<CreateTransactionRequest>
{
    private readonly IXmlProcessingService _xmlProcessingService;

    public CreateTransactionEndpoint(IXmlProcessingService xmlProcessingService)
    {
        _xmlProcessingService = xmlProcessingService;
    }

    public override void Configure()
    {
        Post("/data/create-transactions");
        AllowFileUploads(); // or omit the param for default behavior
        Permissions("CAN_CREATE_TRANSACTION"); 
        Description(b =>
        {
            b.WithName("CreateTransaction");
            b.Produces(200);
            b.ProducesProblem(400);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary
        {
            Description = "Upload and process one or more XML transaction files."
        });
    }

    public override async Task HandleAsync(CreateTransactionRequest req, CancellationToken ct)
    {
        var files = req.Files;
        if (files.Count == 0)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var output = new List<string>();
        foreach (var file in files)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var xmlContent = await reader.ReadToEndAsync();
            var bson = await _xmlProcessingService.ProcessAsync(xmlContent);
            output.Add($"Inserted: {file.FileName} id: {bson["_id"]}");
        }

        await SendOkAsync(output, ct);
    }
}
