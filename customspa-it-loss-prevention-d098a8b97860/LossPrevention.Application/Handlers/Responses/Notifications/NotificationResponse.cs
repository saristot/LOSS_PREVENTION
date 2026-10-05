namespace LossPrevention.Application.Handlers.Responses.Notifications
{
    public sealed class NotificationResponse
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "message";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string From { get; set; } = "";
        public string FromName { get; set; } = "";
        public string To { get; set; } = "";
        public string ToType { get; set; } = "user";
        public string ReplyTo { get; set; } = "";
        public bool IsRead { get; set; } = false;
        public string CreatedAt { get; set; } = "";
    }
}
