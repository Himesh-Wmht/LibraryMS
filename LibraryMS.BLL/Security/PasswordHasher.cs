using System;
using System.Security.Cryptography;

namespace LibraryMS.BLL.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;   // 128 bits
        private const int HashSize = 32;   // 256 bits (SHA-256)
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var salt = GenerateSalt();
            var hash = GenerateHash(password, salt);

            // store as: base64Salt:base64Hash
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt;
            byte[] hash;

            try
            {
                salt = Convert.FromBase64String(parts[0]);
                hash = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return false;
            }

            var newHash = GenerateHash(password, salt);

            // Constant-time compare (better than SlowEquals in new .NET)
            return CryptographicOperations.FixedTimeEquals(hash, newHash);
        }

        public static bool LooksHashed(string storedPassword)
        {
            return !string.IsNullOrWhiteSpace(storedPassword) && storedPassword.Contains(':');
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt); // .NET 6+ recommended
            return salt;
        }

        private static byte[] GenerateHash(string password, byte[] salt)
        {
            // Same PBKDF2 approach as your MPWeb class, SHA-256
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(HashSize);
        }
    }
}
