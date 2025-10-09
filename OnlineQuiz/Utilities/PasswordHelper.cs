using BCrypt.Net;

namespace OnlineQuiz.Utilities
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string password)
        {
            // Allow overriding work factor via environment for test speed
            var workFactor = GetWorkFactor();
            var salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        /// <summary>
        /// Verifies a password against its hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hashedPassword">Hashed password</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        /// <summary>
        /// Reads BCrypt work factor from environment variable 'BCRYPT_WORK_FACTOR'.
        /// Defaults to 10 if not set or invalid. Clamped between 4 and 31.
        /// </summary>
        private static int GetWorkFactor()
        {
            var value = Environment.GetEnvironmentVariable("BCRYPT_WORK_FACTOR");
            if (int.TryParse(value, out var work))
            {
                if (work < 4) work = 4;
                if (work > 31) work = 31;
                return work;
            }
            return 10; // sensible default for production
        }
    }
}