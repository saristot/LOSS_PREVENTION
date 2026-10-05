using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.User.Users;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.API.Endpoints.User.Users
{
    public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest, string>
    {
        private readonly IPasswordResetService _passwordResetService;

        public ResetPasswordEndpoint(IPasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        public override void Configure()
        {
            Post("/users/reset-password");
            AllowAnonymous();
            Options(x =>
            {
                x.WithName("ResetPassword");
                x.Produces<string>(200);
                x.Produces(400);
            });
            Summary(s =>
            {
                s.Summary = "Reset password with token";
                s.Description = "Reset user password using a valid reset token";
                s.ResponseExamples[200] = "Password reset successfully";
            });
        }

        public override async Task HandleAsync(ResetPasswordRequest request, CancellationToken ct)
        {
            try
            {
                // Validate password strength (optional - add your own rules)
                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                {
                    await SendStringAsync("Password must be at least 8 characters long.", 400, cancellation: ct);
                    return;
                }

                await _passwordResetService.ResetPasswordWithTokenAsync(request.Token, request.NewPassword);
                await SendOkAsync("Password reset successfully. You can now login with your new password.", ct);
            }
            catch (InvalidOperationException ex)
            {
                await SendStringAsync(ex.Message, 400, cancellation: ct);
            }
            catch (Exception)
            {
                await SendStringAsync("An error occurred while resetting your password.", 500, cancellation: ct);
            }
        }
    }
}