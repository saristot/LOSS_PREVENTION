using LossPrevention.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace LossPrevention.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _frontendUrl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "25");
            _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@lossprevention.local";
            _fromName = _configuration["Email:FromName"] ?? "Loss Prevention System";
            _frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:5173";
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string username)
        {
            var resetLink = $"{_frontendUrl}/reset-password?token={resetToken}";

            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Password Reset Request</h2>
                    <p>Hello {username},</p>
                    <p>We received a request to reset your password. Click the link below to reset your password:</p>
                    <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-decoration: none; display: inline-block; border-radius: 4px;'>Reset Password</a></p>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{resetLink}</p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Loss Prevention Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.EnableSsl = false; // smtp4dev doesn't use SSL
                smtpClient.UseDefaultCredentials = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log the exception in production
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}