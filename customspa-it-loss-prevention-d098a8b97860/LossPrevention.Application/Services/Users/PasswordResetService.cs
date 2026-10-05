using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Infrastructure.Helpers;
using LossPrevention.Infrastructure.Interfaces;
using LossPrevention.Infrastructure.Repositories;
using LossPrevention.Infrastructure.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace LossPrevention.Application.Services.Users
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IMongoRepository<User> _userRepository;
        private readonly IMongoRepository<PasswordResetToken> _resetTokenRepository;
        private readonly IEmailService _emailService;
        private const int TokenExpiryHours = 1;

        public PasswordResetService(
            IMongoRepository<User> userRepository,
            IMongoRepository<PasswordResetToken> resetTokenRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _resetTokenRepository = resetTokenRepository;
            _emailService = emailService;
        }

        public async Task<string> GenerateResetTokenAsync(string usernameOrEmail)
        {
            // Try to find user by username or email
            var user = await _userRepository.FindOneAsync(u =>
                u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException("User account is inactive.");
            }

            // Invalidate any existing tokens for this user
            var existingTokensFilter = Builders<PasswordResetToken>.Filter.And(
                Builders<PasswordResetToken>.Filter.Eq(t => t.UserId, user._id),
                Builders<PasswordResetToken>.Filter.Eq(t => t.IsUsed, false)
            );

            var updateDefinition = Builders<PasswordResetToken>.Update.Set(t => t.IsUsed, true);
            await _resetTokenRepository.UpdateManyAsync(existingTokensFilter, updateDefinition);

            // Generate a secure random token
            var token = GenerateSecureToken();

            // Create new reset token — store only the hash, email the raw token
            var resetToken = new PasswordResetToken
            {
                TokenHash = HashToken(token),
                UserId = user._id,
                Email = user.Email,
                ExpiryDate = DateTime.UtcNow.AddHours(TokenExpiryHours),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _resetTokenRepository.InsertOneAsync(resetToken);

            // Send email
            await _emailService.SendPasswordResetEmailAsync(user.Email, token, user.Username);

            return token;
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            var tokenHash = HashToken(token);
            var resetToken = await _resetTokenRepository.FindOneAsync(t => t.TokenHash == tokenHash);

            if (resetToken == null)
            {
                return false;
            }

            if (resetToken.IsUsed)
            {
                return false;
            }

            if (resetToken.ExpiryDate < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword)
        {
            var tokenHash = HashToken(token);
            var resetToken = await _resetTokenRepository.FindOneAsync(t => t.TokenHash == tokenHash);

            if (resetToken == null)
            {
                throw new InvalidOperationException("Invalid reset token.");
            }

            if (resetToken.IsUsed)
            {
                throw new InvalidOperationException("Reset token has already been used.");
            }

            if (resetToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Reset token has expired.");
            }

            // Hash the new password
            var passwordHash = PasswordHasher.HashPassword(newPassword);

            // Update user's password
            var userFilter = Builders<User>.Filter.Eq("_id", resetToken.UserId);
            var userUpdateDefinition = Builders<User>.Update
                .Set(u => u.PasswordHash, passwordHash.Hash)
                .Set(u => u.PasswordSalt, passwordHash.Salt);

            var userUpdateResult = await _userRepository.UpdateOneAsync(userFilter, userUpdateDefinition);

            if (userUpdateResult.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Failed to update password.");
            }

            // Mark token as used
            var tokenFilter = Builders<PasswordResetToken>.Filter.Eq("_id", resetToken._id);
            var tokenUpdateDefinition = Builders<PasswordResetToken>.Update.Set(t => t.IsUsed, true);
            await _resetTokenRepository.UpdateOneAsync(tokenFilter, tokenUpdateDefinition);

            return true;
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string HashToken(string token)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}