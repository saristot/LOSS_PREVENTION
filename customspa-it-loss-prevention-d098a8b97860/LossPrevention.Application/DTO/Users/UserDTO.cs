namespace LossPrevention.Application.DTO.Users
{
    public sealed class UserDTO
    {
        public string _id { get; set; } = string.Empty;
        public required string Username { get; set; }
        public required string PasswordHash { get; set; } = string.Empty;
        public required string PasswordSalt { get; set; } = string.Empty;
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime RegistrationDate { get; set; }
        public required bool IsActive { get; set; } = true;
        public List<string> Roles { get; set; } = new List<string>();

        public string? LockField { get; set; }
        public string? LockValue { get; set; }
    }
}
