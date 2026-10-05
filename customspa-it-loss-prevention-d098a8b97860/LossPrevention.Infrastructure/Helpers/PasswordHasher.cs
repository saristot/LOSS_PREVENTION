using System.Security.Cryptography;

namespace LossPrevention.Infrastructure.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int CurrentIterations = 600_000;
        private const int LegacyIterations = 10_000;

        // New hashes are prefixed "v2:{iterations}:{base64hash}".
        // Legacy hashes are plain base64 (no prefix) — produced with 10,000 iterations.
        public static (string Hash, string Salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] saltBytes = new byte[SaltSize];
            rng.GetBytes(saltBytes);
            string salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, CurrentIterations, HashAlgorithmName.SHA256);
            string hash = $"v2:{CurrentIterations}:{Convert.ToBase64String(pbkdf2.GetBytes(HashSize))}";
            return (hash, salt);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            int iterations;
            string base64Hash;

            if (storedHash.StartsWith("v2:", StringComparison.Ordinal))
            {
                var parts = storedHash.Split(':');
                iterations = int.Parse(parts[1]);
                base64Hash = parts[2];
            }
            else
            {
                iterations = LegacyIterations;
                base64Hash = storedHash;
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            string computedHash = Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            return string.Equals(computedHash, base64Hash, StringComparison.Ordinal);
        }

        public static bool IsLegacyHash(string storedHash)
            => !storedHash.StartsWith("v2:", StringComparison.Ordinal);
    }
}
