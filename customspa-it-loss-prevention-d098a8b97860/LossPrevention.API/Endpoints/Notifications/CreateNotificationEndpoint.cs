using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Notifications;
using LossPrevention.Application.Handlers.Responses.Notifications;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Notifications;
using LossPrevention.Infrastructure.Repositories;

namespace LossPrevention.API.Endpoints.Notifications
{
    public sealed class CreateNotificationEndpoint : Endpoint<CreateNotificationRequest, NotificationResponse>
    {
        private readonly IMongoRepository<NotificationDocument> _repo;

        public CreateNotificationEndpoint(IMongoRepository<NotificationDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Post("/notifications");
            Permissions("CAN_MANAGE_NOTIFICATIONS");
            Summary(s =>
            {
                s.Summary = "Create a notification";
                s.Description = "Creates a new notification and sends it to specified recipients.";
            });
        }

        public override async Task HandleAsync(CreateNotificationRequest req, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var doc = req.ToDocument(now);
            await _repo.InsertOneAsync(doc);
            await SendOkAsync(doc.ToResponse(), ct);
        }
    }
   
}
