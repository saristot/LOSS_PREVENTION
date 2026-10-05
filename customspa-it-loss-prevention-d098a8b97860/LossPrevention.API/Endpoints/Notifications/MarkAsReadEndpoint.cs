using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Notifications;
using LossPrevention.Domain.Notifications;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.API.Endpoints.Notifications
{
    public sealed class MarkAsReadEndpoint : Endpoint<MarkAsReadRequest>
    {
        private readonly IMongoRepository<NotificationDocument> _repo;

        public MarkAsReadEndpoint(IMongoRepository<NotificationDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Put("/notifications/{id}/read");
            Permissions("CAN_MANAGE_NOTIFICATIONS");
            Summary(s => s.Summary = "Mark a notification as read");
        }

        public override async Task HandleAsync(MarkAsReadRequest req, CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(idStr) || !ObjectId.TryParse(idStr, out var oid))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, oid);
            var update = Builders<NotificationDocument>.Update.Set(x => x.IsRead, true);
            
            var result = await _repo.UpdateOneAsync(filter, update);
            
            if (result.MatchedCount == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(ct);
        }
    }
}
