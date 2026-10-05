using LossPrevention.Domain.Entities.Users;
using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.User
{
    public interface IUserService
    {
        Task<LossPrevention.Domain.Entities.Users.User> CreateUserAsync(LossPrevention.Domain.Entities.Users.User user, string password);
        Task<bool> UpdateUserAsync(LossPrevention.Domain.Entities.Users.User user);
        Task<bool> DeleteUserAsync(ObjectId id);
        Task<LossPrevention.Domain.Entities.Users.User?> GetUserByIdAsync(ObjectId id);
        Task<LossPrevention.Domain.Entities.Users.User?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<LossPrevention.Domain.Entities.Users.User>> GetAllUsersAsync();
        Task<bool> ChangePasswordAsync(ObjectId id, string newPassword);
        bool ValidatePasswordAsync(LossPrevention.Domain.Entities.Users.User user, string password);
        Task<UserWithRolesAndPermissions> GetUserWithRolesAndPermissions(ObjectId id);
        Task<LossPrevention.Domain.Entities.Users.User> LoginAsync(string username, string password);
    }
}