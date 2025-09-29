using System.Security.Cryptography;
using System.Text;

namespace OnlineQuiz.Utilities
{
    /// <summary>
    /// Utility class for securely hashing and verifying refresh tokens
    /// Uses HMAC-SHA256 with a server-side pepper for one-way hashing
    /// </summary>
    public static class RefreshTokenHasher
    {
        /// <summary>
        /// Generates a cryptographically secure refresh token
        /// </summary>
        /// <param name="numBytes">Number of random bytes to generate (default: 64)</param>
        /// <returns>Base64 encoded refresh token</returns>
        public static string GenerateRefreshTokenBase64(int numBytes = 64)
        {
            var bytes = RandomNumberGenerator.GetBytes(numBytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Creates a one-way HMAC-SHA256 hash of the refresh token
        /// </summary>
        /// <param name="token">The plaintext refresh token</param>
        /// <param name="pepper">Server-side pepper for additional security</param>
        /// <returns>Hex string of the HMAC hash (64 characters)</returns>
        public static string HashToken(string token, byte[] pepper)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));
            
            if (pepper == null || pepper.Length == 0)
                throw new ArgumentException("Pepper cannot be null or empty", nameof(pepper));

            using var hmac = new HMACSHA256(pepper);
            var mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(mac);
        }

        /// <summary>
        /// Verifies a refresh token against its stored hash using constant-time comparison
        /// </summary>
        /// <param name="token">The plaintext token to verify</param>
        /// <param name="storedHash">The stored hash from the database</param>
        /// <param name="pepper">Server-side pepper used for hashing</param>
        /// <returns>True if the token matches the hash, false otherwise</returns>
        public static bool VerifyToken(string token, string storedHash, byte[] pepper)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                var computedHash = HashToken(token, pepper);
                var storedHashBytes = Convert.FromHexString(storedHash);
                var computedHashBytes = Convert.FromHexString(computedHash);
                
                // Use constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the pepper from environment variables or configuration
        /// </summary>
        /// <returns>Pepper bytes for HMAC</returns>
        public static byte[] GetPepper()
        {
            var pepperBase64 = Environment.GetEnvironmentVariable("REFRESH_TOKEN_PEPPER");
            
            if (string.IsNullOrEmpty(pepperBase64))
            {
                throw new InvalidOperationException(
                    "REFRESH_TOKEN_PEPPER environment variable is not set. " +
                    "Generate a secure random value: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");
            }

            try
            {
                return Convert.FromBase64String(pepperBase64);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "REFRESH_TOKEN_PEPPER environment variable is not a valid Base64 string");
            }
        }
    }
}