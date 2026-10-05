using LossPrevention.Domain.Entities.Users;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Interfaces.User
{
    public interface IUserRoleService
    {
        // User Role Management
        Task<bool> AddUserToRoleAsync(ObjectId userId, ObjectId roleId);
        Task<bool> RemoveRoleFromUserAsync(ObjectId userId, ObjectId roleId);
        Task<List<ObjectId>> GetUserRolesAsync(ObjectId id);
        Task<bool> UserIsInRoleAsync(ObjectId id, string roleName);

        // Role Management
        Task<Role> CreateRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(ObjectId id);
        Task<Role?> GetRoleByIdAsync(ObjectId id);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}