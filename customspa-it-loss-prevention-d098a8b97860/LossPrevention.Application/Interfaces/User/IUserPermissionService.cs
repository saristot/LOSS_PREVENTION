using LossPrevention.Domain.Entities.Users;
using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.User
{
    public interface IUserPermissionService
    {
        // User Permissions
        Task<bool> UserHasPermissionAsync(ObjectId id, string permissionName);

        // Role Permission Management
        Task AddPermissionToRoleAsync(ObjectId roleId, ObjectId permissionId);
        Task RemovePermissionFromRoleAsync(ObjectId roleId, ObjectId permissionId);
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(ObjectId roleId);

        // Permissions
        Task<Permission?> GetPermissionByIdAsync(ObjectId id);
        Task<Permission> CreatePermissionAsync(Permission permission);
        Task<bool> UpdatePermissionAsync(Permission permission);
        Task<bool> DeletePermissionAsync(ObjectId id);
        Task<Permission?> GetPermissionByNameAsync(string permissionName);
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    }
}
