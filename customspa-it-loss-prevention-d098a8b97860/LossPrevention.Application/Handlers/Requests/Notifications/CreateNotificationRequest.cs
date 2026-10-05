namespace LossPrevention.Application.Handlers.Requests.Notifications
{
    public sealed class CreateNotificationRequest
    {
        public string Type { get; set; } = "message"; // message, alert, reply
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string From { get; set; } = ""; // User ID
        public string FromName { get; set; } = "";
        public List<string> To { get; set; } = []; // User ID, Role ID, or Group ID
        public string ToType { get; set; } = "user"; // user, role, group
        public string ReplyTo { get; set; } = ""; // Original notification ID if this is a reply
    }
}
