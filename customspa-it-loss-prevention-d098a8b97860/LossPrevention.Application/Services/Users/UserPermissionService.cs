using LossPrevention.Domain.Entities.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.Users
{
    public sealed class UserPermissionService : IUserPermissionService
    {
        private readonly IMongoRepository<User> _userRepository;
        private readonly IMongoRepository<Role> _roleRepository;
        private readonly IMongoRepository<Permission> _permissionRepository;
        private readonly IUserService _userService;
        private readonly IUserRoleService _userRoleService;

        public UserPermissionService(
            IMongoRepository<LossPrevention.Domain.Entities.Users.User> userRepository,
            IMongoRepository<Role> roleRepository,
            IMongoRepository<Permission> permissionRepository,
            IUserService userService,
            IUserRoleService userRoleService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _userService = userService;
            _userRoleService = userRoleService;
        }

        // Public Role Permission Management Methods
        public async Task AddPermissionToRoleAsync(ObjectId roleId, ObjectId permissionId)
        {
            var role = await _userRoleService.GetRoleByIdAsync(roleId); // Assuming you have a GetByRoleIdAsync method
            var permission = await GetPermissionByIdAsync(permissionId); // Assuming you have a GetByPermissionIdAsync method

            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID '{roleId}' not found.");
            }
            if (permission == null)
            {
                throw new KeyNotFoundException($"Permission with ID '{permissionId}' not found.");
            }

            if (role.Permissions == null)
            {
                role.Permissions = new List<ObjectId>(); // Permissions in Role is a list of integers (PermissionIds)
            }

            if (!role.Permissions.Contains(permissionId))
            {
                role.Permissions.Add(permissionId);
                var filter = Builders<Role>.Filter.Eq("_id", role._id); // Filter by RoleId
                var updateDefinition = Builders<Role>.Update.Set(r => r.Permissions, role.Permissions);
                await _roleRepository.UpdateOneAsync(filter, updateDefinition);
            }
        }
        public async Task RemovePermissionFromRoleAsync(ObjectId roleId, ObjectId permissionId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId); // Assuming Role ID is string
            if (role != null && role.Permissions != null)
            {
                var removed = role.Permissions.RemoveAll(p => p == permissionId); // Assuming Permission ID is string
                if (removed > 0)
                {
                    var filter = Builders<Role>.Filter.Eq("_id", role._id);
                    var updateDefinition = Builders<Role>.Update.Set(r => r.Permissions, role.Permissions);
                    await _roleRepository.UpdateOneAsync(filter, updateDefinition);
                }
            }
        }
        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(ObjectId id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role?.Permissions == null || !role.Permissions.Any())
            {
                return new List<Permission>();
            }

            var permissions = await _permissionRepository.FindManyAsync(p => role.Permissions.Contains(p._id));
            return permissions;
        }
        public async Task<Permission?> GetPermissionByIdAsync(ObjectId id)
        {
            return await _permissionRepository.GetByIdAsync(id); // Assuming Permission ID is string
        }

        // Public Permission Management Methods
        public async Task<Permission> CreatePermissionAsync(Permission permission)
        {
            var filter = Builders<Permission>.Filter.Eq("PermissionName", permission.PermissionName);
            var existingPermission = await _permissionRepository.FindOneAsync(filter);
            if (existingPermission != null)
            {
                throw new InvalidOperationException($"Permission with name '{permission.PermissionName}' already exists.");
            }
            permission._id = new ObjectId();
            await _permissionRepository.InsertOneAsync(permission);
            return permission;
        }
        public async Task<bool> UpdatePermissionAsync(Permission permission)
        {
            var filter = Builders<Permission>.Filter.Eq("_id", permission._id);
            var existingPermission = await _permissionRepository.FindOneAsync(filter);
            if (existingPermission == null)
            {
                throw new KeyNotFoundException($"Permission with ID '{permission._id}' not found.");
            }
            var updateDefinition = Builders<Permission>.Update
                .Set(p => p.PermissionName, permission.PermissionName)
                .Set(p => p.PermissionText, permission.PermissionText)
                .Set(p => p.Description, permission.Description);
            var updateResult = await _permissionRepository.UpdateOneAsync(filter, updateDefinition);
            return updateResult.ModifiedCount == 0 ? false : true;
        }
        public async Task<bool> DeletePermissionAsync(ObjectId id)
        {
            // Find all roles that have this permission and remove it
            var filter = Builders<Role>.Filter.ElemMatch(r => r.Permissions, p => p == id); // Assuming Permission ID is string
            var rolesWithPermission = await _roleRepository.FindManyAsync(filter);

            foreach (var role in rolesWithPermission)
            {
                role.Permissions?.RemoveAll(p => p == id);
                var updateFilter = Builders<Role>.Filter.Eq("_id", role._id);
                var updateDefinition = Builders<Role>.Update.Set(r => r.Permissions, role.Permissions);
                await _roleRepository.UpdateOneAsync(updateFilter, updateDefinition);
            }

            var deleteResult = await _permissionRepository.DeleteByIdAsync(id); // Assuming Permission ID is string
            return deleteResult.DeletedCount == 0 ? false : true;
        }
        public async Task<Permission?> GetPermissionByNameAsync(string permissionName)
        {
            var filter = Builders<Permission>.Filter.Eq("PermissionName", permissionName);
            return await _permissionRepository.FindOneAsync(filter);
        }
        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            var result = await _permissionRepository.GetAllAsync();
            return result;
        }
        public async Task<bool> UserHasPermissionAsync(ObjectId id, string permissionName)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.Roles == null || !user.Roles.Any())
            {
                return false;
            }

            var allPermissions = new List<string>();
            foreach (var roleId in user.Roles)
            {
                var fetchedRole = await _roleRepository.GetByIdAsync(roleId);
                if (fetchedRole?.Permissions != null)
                {
                    // Assuming fetchedRole.Permissions is List<ObjectId> referencing Permission documents
                    foreach (var permissionId in fetchedRole.Permissions)
                    {
                        var permission = await _permissionRepository.GetByIdAsync(permissionId);
                        if (permission?.PermissionName != null)
                        {
                            allPermissions.Add(permission.PermissionName);
                        }
                    }
                }
            }

            return allPermissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
