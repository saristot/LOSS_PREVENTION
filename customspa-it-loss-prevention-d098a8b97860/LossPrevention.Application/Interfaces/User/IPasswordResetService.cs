using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.User
{
    public interface IPasswordResetService
    {
        Task<string> GenerateResetTokenAsync(string usernameOrEmail);
        Task<bool> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword);
    }
}
