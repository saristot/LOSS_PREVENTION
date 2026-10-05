namespace LossPrevention.Application.Handlers.Requests.Notifications
{
    public sealed class ReplyToNotificationRequest
    {
        public string Message { get; set; } = "";
        public string From { get; set; } = "";
        public string FromName { get; set; } = "";
    }
}
