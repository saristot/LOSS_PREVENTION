using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Notifications;
using LossPrevention.Domain.Notifications;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Notifications
{
    public sealed class DeleteNotificationEndpoint : Endpoint<DeleteNotificationRequest>
    {
        private readonly IMongoRepository<NotificationDocument> _repo;

        public DeleteNotificationEndpoint(IMongoRepository<NotificationDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Delete("/notifications/{id}");
            Permissions("CAN_MANAGE_NOTIFICATIONS");
            Summary(s =>
            {
                s.Summary = "Delete a notification";
            });
        }

        public override async Task HandleAsync(DeleteNotificationRequest req, CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(idStr) || !ObjectId.TryParse(idStr, out var oid))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var result = await _repo.DeleteByIdAsync(oid);
            if (result.DeletedCount == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(ct);
        }
    }
}
