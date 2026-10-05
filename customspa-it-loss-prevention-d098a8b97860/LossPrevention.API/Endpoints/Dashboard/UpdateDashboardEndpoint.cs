using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Dashboard;
using LossPrevention.Application.Handlers.Responses.Dashboard;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Dashboards;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace LossPrevention.API.Handlers.Dashboards
{
    // NOTE: Domain model uses string Id/WorkspaceId with [BsonRepresentation(BsonType.ObjectId)]
    // and TabId is a plain string (e.g., "tab-1"). Keep everything as strings here.
    public sealed class UpdateDashboardEndpoint : Endpoint<UpdateDashboardRequest, DashboardResponse>
    {
        private readonly IMongoRepository<DashboardDocument> _repo;

        public UpdateDashboardEndpoint(IMongoRepository<DashboardDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Put("/dashboards/{id}");
            Permissions("CAN_MANAGE_DASHBOARDS");
            Summary(s => s.Summary = "Update a dashboard");
        }

        public override async Task HandleAsync(UpdateDashboardRequest req, CancellationToken ct)
        {
            // Route param is a string. Validate it's a legal ObjectId, since the DB will store it as ObjectId.
            var idStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(idStr) || !ObjectId.TryParse(idStr, out _))
            {
                AddError("id", "Invalid dashboard id.");
                await SendErrorsAsync(400, ct);
                return;
            }

            // Convert JsonElement objects in data properties to proper types
            if (req.Blocks != null)
            {
                foreach (var block in req.Blocks)
                {
                    if (block.data != null)
                    {
                        var convertedData = new Dictionary<string, object>();
                        foreach (var kvp in block.data)
                        {
                            convertedData[kvp.Key] = ConvertJsonElement(kvp.Value);
                        }
                        block.data = convertedData;
                    }
                }
            }

            var filter = Builders<DashboardDocument>.Filter.Eq(x => x.Id, ObjectId.Parse(idStr));
            var existing = await _repo.FindOneAsync(filter);
            if (existing is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Apply changes (keeps WorkspaceId/TabId as strings; blocks mapped to BsonDocument)
            existing.Apply(req, DateTime.UtcNow);

            // Replace document by Id (string). Simpler than building a big Update<...>
            // Replace document by Id (string). Use UpdateDefinition to specify the update operation.
            var updateDefinition = Builders<DashboardDocument>.Update
                .Set(d => d.Name, existing.Name)
                .Set(d => d.WorkspaceId, existing.WorkspaceId)
                .Set(d => d.TabId, existing.TabId)
                .Set(d => d.Blocks, existing.Blocks)
                .Set(d => d.CreatedAtUtc, existing.CreatedAtUtc)
                .Set(d => d.UpdatedAtUtc, existing.UpdatedAtUtc);

            await _repo.UpdateOneAsync(filter, updateDefinition);

            await SendOkAsync(existing.ToResponse(), ct);
        }

        private static object ConvertJsonElement(object value)
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString(),
                    JsonValueKind.Number => jsonElement.TryGetInt32(out var intVal) ? intVal : jsonElement.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Object => jsonElement.Deserialize<Dictionary<string, object>>(),
                    JsonValueKind.Array => jsonElement.Deserialize<object[]>(),
                    _ => jsonElement.ToString()
                };
            }
            return value;
        }
    }
}
