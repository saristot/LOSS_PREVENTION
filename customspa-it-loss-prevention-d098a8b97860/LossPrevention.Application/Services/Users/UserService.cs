using LossPrevention.Domain.Entities.Users;
using LossPrevention.Infrastructure.Helpers;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Data;
using System.Security.Cryptography;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;

namespace LossPrevention.Application.Services.Users
{

    public class UserService : IUserService
    {
        private readonly IMongoRepository<User> _userRepository;
        private readonly IMongoRepository<Role> _roleRepository;
        private readonly IMongoRepository<Permission> _permissionRepository; // Add Permission Repository

        public UserService(
            IMongoRepository<LossPrevention.Domain.Entities.Users.User> userRepository,
            IMongoRepository<Role> roleRepository,
            IMongoRepository<Permission> permissionRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        // Private Utility Methods
        private bool VerifyPasswordAsync(string password, string hashedPassword, string salt)
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            try
            {
                return PasswordHasher.VerifyPassword(password, hashedPassword, salt);
            }
            catch (FormatException)
            {
                return false; // Handle potential base64 decoding errors
            }
            catch (CryptographicException)
            {
                return false; // Handle potential cryptographic errors
            }
        }

        // Public User Management Methods
        public async Task<LossPrevention.Domain.Entities.Users.User> CreateUserAsync(LossPrevention.Domain.Entities.Users.User user, string password)
        {
            // Check for existing user by Username
            var existingUserByUsername = await _userRepository.FindOneAsync(u => u.Username == user.Username);
            if (existingUserByUsername != null)
            {
                throw new InvalidOperationException($"Username '{user.Username}' is already taken.");
            }

            // Check for existing user by Email
            var existingUserByEmail = await _userRepository.FindOneAsync(u => u.Email == user.Email);
            if (existingUserByEmail != null)
            {
                throw new InvalidOperationException($"Email '{user.Email}' is already taken.");
            }

            var passwordResult = PasswordHasher.HashPassword(password);
            user.PasswordHash = passwordResult.Hash;
            user.PasswordSalt = passwordResult.Salt;
            user.RegistrationDate = DateTime.UtcNow;

            await _userRepository.InsertOneAsync(user);

            return user;
        }

        public async Task<bool> UpdateUserAsync(LossPrevention.Domain.Entities.Users.User user)
        {
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{user._id}' not found.");
            }

            var filter = Builders<LossPrevention.Domain.Entities.Users.User>.Filter.Eq("_id", user._id);

            var updateBuilder = Builders<LossPrevention.Domain.Entities.Users.User>.Update;
            var updates = new List<UpdateDefinition<LossPrevention.Domain.Entities.Users.User>>
            {
                updateBuilder.Set(u => u.Username, user.Username),
                updateBuilder.Set(u => u.Email, user.Email),
                updateBuilder.Set(u => u.FirstName, user.FirstName),
                updateBuilder.Set(u => u.LastName, user.LastName),
                updateBuilder.Set(u => u.RegistrationDate, user.RegistrationDate),
                updateBuilder.Set(u => u.IsActive, user.IsActive),
                updateBuilder.Set(u => u.Roles, user.Roles)
            };

            if (!string.IsNullOrEmpty(user.LockField) && !string.IsNullOrEmpty(user.LockValue))
            {
                updates.Add(updateBuilder.Set(u => u.LockField, user.LockField));
                updates.Add(updateBuilder.Set(u => u.LockValue, user.LockValue));
            }

            var updateDefinition = updateBuilder.Combine(updates);

            var updateResult = await _userRepository.UpdateOneAsync(filter, updateDefinition);
            return updateResult.ModifiedCount > 0;
        }


        public async Task<bool> DeleteUserAsync(ObjectId id)
        {
            var deleteUserResult = await _userRepository.DeleteByIdAsync(id);
            return deleteUserResult.DeletedCount == 0 ? false : true;
        }

        public async Task<LossPrevention.Domain.Entities.Users.User?> GetUserByIdAsync(ObjectId id)
        {
            var filter = Builders<LossPrevention.Domain.Entities.Users.User>.Filter.Eq("_id", id);
            return await _userRepository.FindOneAsync(filter);
        }

        public async Task<LossPrevention.Domain.Entities.Users.User?> GetUserByUsernameAsync(string username)
        {
            var filter = Builders<LossPrevention.Domain.Entities.Users.User>.Filter.Eq("Username", username);
            return await _userRepository.FindOneAsync(filter);
        }

        public async Task<IEnumerable<LossPrevention.Domain.Entities.Users.User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<bool> ChangePasswordAsync(ObjectId id, string newPassword)
        {
            var passwordHash = PasswordHasher.HashPassword(newPassword);
            var filter = Builders<LossPrevention.Domain.Entities.Users.User>.Filter.Eq("_id", id);
            var updateDefinition = Builders<LossPrevention.Domain.Entities.Users.User>.Update
                .Set(u => u.PasswordHash, passwordHash.Hash)
                .Set(u => u.PasswordSalt, passwordHash.Salt);

            var updateResult = await _userRepository.UpdateOneAsync(filter, updateDefinition);
            return updateResult.ModifiedCount == 0 ? false : true;
        }

        public bool ValidatePasswordAsync(LossPrevention.Domain.Entities.Users.User user, string password)
        {
            if (user == null)
            {
                return false;
            }
            return VerifyPasswordAsync(password, user.PasswordHash, user.PasswordSalt);
        }

        // Public Authorization Method
        public async Task<UserWithRolesAndPermissions> GetUserWithRolesAndPermissions(ObjectId id)
        {
            //TODO: Check roles are pulled from the list
            var userFilter = Builders<LossPrevention.Domain.Entities.Users.User>.Filter.Eq("_id", id);
            var user = await _userRepository.FindOneAsync(userFilter);
            if (user == null)
            {
                return null;
            }

            // Assuming User.RoleIds contains string representations of Role _id
            var roleObjectIds = user.Roles.Select(x => new ObjectId(x.ToString())).ToList();
            var rolesFilter = Builders<Role>.Filter.In("_id", roleObjectIds);
            var roles = await _roleRepository.FindManyAsync(rolesFilter);

            var permissionIds = roles.SelectMany(r => r.Permissions).Distinct().ToList();
            var permissionsFilter = Builders<Permission>.Filter.In("PermissionId", permissionIds);
            var permissions = await _permissionRepository.FindManyAsync(permissionsFilter);

            return new UserWithRolesAndPermissions
            {
                User = user,
                Roles = roles,
                Permissions = permissions
            };
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _userRepository.FindOneAsync(u => u.Username == username);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive.");
            }

            var isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            // Upgrade legacy 10k-iteration hashes to 600k on successful login
            if (PasswordHasher.IsLegacyHash(user.PasswordHash))
            {
                var upgraded = PasswordHasher.HashPassword(password);
                var upgradeFilter = Builders<User>.Filter.Eq("_id", user._id);
                var upgradeUpdate = Builders<User>.Update
                    .Set(u => u.PasswordHash, upgraded.Hash)
                    .Set(u => u.PasswordSalt, upgraded.Salt);
                await _userRepository.UpdateOneAsync(upgradeFilter, upgradeUpdate);
            }

            return user;
        }

    }
}