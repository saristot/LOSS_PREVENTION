using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.Users
{
    public sealed class UserRoleService : IUserRoleService
    {
        private readonly IMongoRepository<User> _userRepository;
        private readonly IMongoRepository<Role> _roleRepository;
        private readonly IMongoRepository<Permission> _permissionRepository;
        private readonly IUserService _userService;

        public UserRoleService(
            IMongoRepository<User> userRepository,
            IMongoRepository<Role> roleRepository,
            IMongoRepository<Permission> permissionRepository,
            IUserService userService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _userService = userService;
        }

        // Public Role Management Methods
        public async Task<Role> CreateRoleAsync(Role role)
        {
            var filter = Builders<Role>.Filter.Eq("RoleName", role.RoleName); // Check by RoleName
            var existingRole = await _roleRepository.FindOneAsync(filter);

            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name '{role.RoleName}' already exists.");
            }

            await _roleRepository.InsertOneAsync(role);
            return role;
        }
        public async Task<bool> UpdateRoleAsync(Role role)
        {
            var filter = Builders<Role>.Filter.Eq("_id", role._id);
            var existingRole = await _roleRepository.FindOneAsync(filter);
            if (existingRole == null)
            {
                throw new KeyNotFoundException($"Role with ID '{role._id}' not found.");
            }

            var updateDefinition = Builders<Role>.Update
                .Set(r => r.Description, role.Description)
                .Set(r => r.Permissions, role.Permissions)
                .Set(r => r.RoleName, role.RoleName);

            var updateResult = await _roleRepository.UpdateOneAsync(filter, updateDefinition);
            return updateResult.ModifiedCount == 0 ? false : true;
        }
        public async Task<bool> DeleteRoleAsync(ObjectId id)
        {
            // Find all users that have this role
            var filter = Builders<User>.Filter.ElemMatch(u => u.Roles, r => r == id);
            var usersWithRole = await _userRepository.FindManyAsync(filter);

            // Iterate through the users and remove the role
            foreach (var user in usersWithRole)
            {
                user.Roles.RemoveAll(role => role == id);
                // Update the user document in the database
                var updateFilter = Builders<User>.Filter.Eq(u => u._id, user._id);
                var updateDefinition = Builders<User>.Update.Set(u => u.Roles, user.Roles);
                await _userRepository.UpdateOneAsync(updateFilter, updateDefinition);
            }

            // Finally, delete the role itself
            var roleFilter = Builders<Role>.Filter.Eq(x => x._id, id);
            var deleteResult = await _roleRepository.DeleteOneAsync(roleFilter);
            return deleteResult.DeletedCount == 0 ? false : true;
        }
        public async Task<Role?> GetRoleByIdAsync(ObjectId id)
        {
            var filter = Builders<Role>.Filter.Eq("_id", id);
            return await _roleRepository.FindOneAsync(filter);
        }
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            var filter = Builders<Role>.Filter.Eq("RoleName", roleName);
            return await _roleRepository.FindOneAsync(filter);
        }
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<bool> AddUserToRoleAsync(ObjectId userId, ObjectId roleId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false; // Or throw an exception
            }

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return false; // Or throw an exception
            }

            if (user.Roles.Any(r => r == roleId))
            {
                return true; // User is already in this role
            }

            user.Roles.Add(roleId);

            var filter = Builders<User>.Filter.Eq("_id", userId);
            var updateDefinition = Builders<User>.Update.Set(u => u.Roles, user.Roles);
            var updateResult = await _userRepository.UpdateOneAsync(filter, updateDefinition);

            return updateResult.ModifiedCount == 0 ? false : true;
        }
        public async Task<bool> RemoveRoleFromUserAsync(ObjectId userId, ObjectId roleId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false; // Or throw an exception
            }

            var removed = user.Roles.RemoveAll(r => r == roleId);

            if (removed > 0)
            {
                var filter = Builders<User>.Filter.Eq("_id", user._id);
                var updateDefinition = Builders<User>.Update.Set(u => u.Roles, user.Roles);
                var updateResult = await _userRepository.UpdateOneAsync(filter, updateDefinition);
                return updateResult.ModifiedCount == 0;
            }

            return true; // Role wasn't found for the user
        }
        public async Task<List<ObjectId>> GetUserRolesAsync(ObjectId id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            return user.Roles ?? new List<ObjectId>();
        }
        public async Task<bool> UserIsInRoleAsync(ObjectId id, string roleName)
        {
            var userRoles = await GetUserRolesAsync(id);
            var role = await GetRoleByNameAsync(roleName);

            if (role == null || userRoles == null)
            {
                return false; // The role doesn't exist, or the user has no roles.
            }

            // Does the user contain this role?
            return userRoles.Any(r => r == role._id);
        }
    }
}
