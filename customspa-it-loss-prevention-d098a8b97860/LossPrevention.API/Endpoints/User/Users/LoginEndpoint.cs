using FastEndpoints;
using LossPrevention.API.Handlers.Requests.Login;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Endpoints.User.Users
{
    public class LoginEndpoint : Endpoint<LoginRequest>
    {
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IUserService _userService;
        private readonly IUserPermissionService _userPermissionService;

        public LoginEndpoint(IOptions<JwtSettings> jwtSettings, IUserService userService, IUserPermissionService userPermissionService)
        {
            _jwtSettings = jwtSettings;
            _userService = userService;
            _userPermissionService = userPermissionService;
        }

        public override void Configure()
        {
            Post("/users/login");
            AllowAnonymous();
        }

        public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
        {
            var result = await _userService.LoginAsync(req.Username, req.Password);
            if (result != null)
            {
                var token = CreateTokenAsync(req.Username, result.Roles, result.LockField, result.LockValue);
                await SendAsync(new { Token = token });
            }
            else
            {
                await SendUnauthorizedAsync();
            }
        }

        private async Task<string> CreateTokenAsync(string username, List<ObjectId> roles, string? lockField, string? lockValue)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username)
            };

            foreach (var role in roles)
            {
                var permissions = await _userPermissionService.GetRolePermissionsAsync(role);

                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permissions", permission.PermissionName));
                }
            }

            if (!string.IsNullOrEmpty(lockField) && !string.IsNullOrEmpty(lockValue))
            {
                claims.Add(new Claim("LockField", lockField));
                claims.Add(new Claim("LockValue", lockValue));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Value.Issuer,
                audience: _jwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.Value.ExpiryHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
