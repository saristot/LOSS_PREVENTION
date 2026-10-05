using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.User.Users;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.API.Endpoints.User.Users
{
    public class ValidateResetTokenEndpoint : Endpoint<ValidateResetTokenRequest, bool>
    {
        private readonly IPasswordResetService _passwordResetService;

        public ValidateResetTokenEndpoint(IPasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        public override void Configure()
        {
            Post("/users/validate-reset-token");
            AllowAnonymous();
            Options(x =>
            {
                x.WithName("ValidateResetToken");
                x.Produces<bool>(200);
            });
            Summary(s =>
            {
                s.Summary = "Validate password reset token";
                s.Description = "Check if a password reset token is valid and not expired";
            });
        }

        public override async Task HandleAsync(ValidateResetTokenRequest request, CancellationToken ct)
        {
            try
            {
                var isValid = await _passwordResetService.ValidateResetTokenAsync(request.Token);
                await SendOkAsync(isValid, ct);
            }
            catch (Exception ex)
            {
                await SendAsync(false, 200, ct);
            }
        }
    }
}
