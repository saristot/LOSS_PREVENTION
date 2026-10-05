using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Notifications;
using LossPrevention.Application.Handlers.Responses.Notifications;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Notifications;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.API.Endpoints.Notifications
{
    public sealed class ReplyToNotificationEndpoint : Endpoint<ReplyToNotificationRequest, NotificationResponse>
    {
        private readonly IMongoRepository<NotificationDocument> _repo;

        public ReplyToNotificationEndpoint(IMongoRepository<NotificationDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Post("/notifications/{id}/reply");
            Permissions("CAN_MANAGE_NOTIFICATIONS");
            Summary(s => s.Summary = "Reply to a notification");
        }

        public override async Task HandleAsync(ReplyToNotificationRequest req, CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(idStr) || !ObjectId.TryParse(idStr, out var oid))
            {
                AddError("id", "Invalid notification id.");
                await SendErrorsAsync(400, ct);
                return;
            }

            // Find the original notification
            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, oid);
            var original = await _repo.FindOneAsync(filter);
            if (original is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Create a reply notification
            var now = DateTime.UtcNow;
            var reply = new NotificationDocument
            {
                Id = ObjectId.GenerateNewId(),
                Type = "reply",
                Title = $"Re: {original.Title}",
                Message = req.Message?.Trim() ?? "",
                From = req.From?.Trim() ?? "",
                FromName = req.FromName?.Trim() ?? "",
                To = new List<string> { original.From }, // Send reply back to the original sender
                ToType = "user",
                ReplyTo = original.Id.ToString(),
                IsRead = false,
                CreatedAtUtc = now
            };

            await _repo.InsertOneAsync(reply);
            await SendOkAsync(reply.ToResponse(), ct);
        }
    }
}
