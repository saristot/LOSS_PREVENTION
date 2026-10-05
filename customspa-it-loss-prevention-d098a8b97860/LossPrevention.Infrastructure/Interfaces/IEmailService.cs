namespace LossPrevention.Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string username);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}