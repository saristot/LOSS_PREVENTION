using FastEndpoints;
using LossPrevention.Application.Handlers.Responses.Notifications;
using LossPrevention.Domain.Notifications;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Driver;

namespace LossPrevention.API.Endpoints.Notifications
{
    public sealed class ListNotificationsEndpoint : EndpointWithoutRequest<List<NotificationResponse>>
    {
        private readonly IMongoRepository<NotificationDocument> _repo;

        public ListNotificationsEndpoint(IMongoRepository<NotificationDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Get("/notifications");
            Permissions("CAN_VIEW_NOTIFICATIONS");
            Summary(s =>
            {
                s.Summary = "List all notifications";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var filter = Builders<NotificationDocument>.Filter.Empty;
            var docs = await _repo.FindManyAsync(filter);

            var resp = docs.Select(n => new NotificationResponse
            {
                Id = n.Id.ToString(),
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                From = n.From,
                FromName = n.FromName,
                To = string.Join(", ", n.To), // Convert List<string> to a comma-separated string
                ToType = n.ToType,
                ReplyTo = n.ReplyTo,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAtUtc.ToString("o")
            })
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

            await SendOkAsync(resp, ct);
        }
    }
}
