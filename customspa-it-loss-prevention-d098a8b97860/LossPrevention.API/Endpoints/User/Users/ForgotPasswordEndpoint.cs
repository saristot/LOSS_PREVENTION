using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.User.Users;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.API.Endpoints.User.Users
{
    public class ForgotPasswordEndpoint : Endpoint<ForgotPasswordRequest, string>
    {
        private readonly IPasswordResetService _passwordResetService;

        public ForgotPasswordEndpoint(IPasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        public override void Configure()
        {
            Post("/users/forgot-password");
            AllowAnonymous();
            Options(x =>
            {
                x.WithName("ForgotPassword");
                x.Produces<string>(200);
                x.Produces(400);
                x.Produces(404);
            });
            Summary(s =>
            {
                s.Summary = "Request password reset";
                s.Description = "Send a password reset email to the user";
                s.ResponseExamples[200] = "Password reset email sent successfully";
            });
        }

        public override async Task HandleAsync(ForgotPasswordRequest request, CancellationToken ct)
        {
            try
            {
                await _passwordResetService.GenerateResetTokenAsync(request.UsernameOrEmail);
            }
            catch (KeyNotFoundException)
            {
                // Swallow silently — don't reveal whether the account exists
            }
            catch (InvalidOperationException ex)
            {
                await SendStringAsync(ex.Message, 400, cancellation: ct);
                return;
            }
            catch (Exception)
            {
                await SendStringAsync("An error occurred while processing your request.", 500, cancellation: ct);
                return;
            }

            await SendOkAsync("If an account exists for that username or email, a password reset link has been sent.", ct);
        }
    }
}