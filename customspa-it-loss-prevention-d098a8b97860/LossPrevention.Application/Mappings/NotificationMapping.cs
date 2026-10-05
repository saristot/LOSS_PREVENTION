using LossPrevention.Application.Handlers.Requests.Notifications;
using LossPrevention.Application.Handlers.Responses.Notifications;
using LossPrevention.Domain.Notifications;
using MongoDB.Bson;

namespace LossPrevention.Application.Mappings
{
    public static class NotificationMapping
    {
        public static NotificationResponse ToResponse(this NotificationDocument n) => new()
        {
            Id = n.Id.ToString(),
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            From = n.From,
            FromName = n.FromName,
            To = string.Join(", ", n.To),  // ? Fixed
            ToType = n.ToType,
            ReplyTo = n.ReplyTo,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAtUtc.ToString("o")
        };

        public static NotificationDocument ToDocument(this CreateNotificationRequest r, DateTime nowUtc)
        {
            return new()
            {
                Id = ObjectId.GenerateNewId(),
                Type = r.Type?.Trim() ?? "message",
                Title = r.Title?.Trim() ?? "",
                Message = r.Message?.Trim() ?? "",
                From = r.From?.Trim() ?? "",
                FromName = r.FromName?.Trim() ?? "",
                To = r.To?.Select(t => t?.Trim() ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>(),
                ToType = r.ToType?.Trim() ?? "user",
                ReplyTo = r.ReplyTo?.Trim() ?? "",
                IsRead = false,
                CreatedAtUtc = nowUtc
            };
        }
    }
}
