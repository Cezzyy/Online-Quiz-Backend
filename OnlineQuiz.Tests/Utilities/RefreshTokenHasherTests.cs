using System;
using System.Security.Cryptography;
using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Utilities
{
    public class RefreshTokenHasherTests
    {
        private static byte[] TestPepper() => RandomNumberGenerator.GetBytes(32);

        [Fact]
        public void GenerateRefreshTokenBase64_ReturnsBase64String()
        {
            var token = RefreshTokenHasher.GenerateRefreshTokenBase64();
            Assert.False(string.IsNullOrWhiteSpace(token));

            // Should be valid Base64
            var bytes = Convert.FromBase64String(token);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void HashToken_ProducesHexString()
        {
            var token = RefreshTokenHasher.GenerateRefreshTokenBase64();
            var pepper = TestPepper();

            var hash = RefreshTokenHasher.HashToken(token, pepper);
            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.Equal(64, hash.Length); // HMAC-SHA256 hex length
        }

        [Fact]
        public void VerifyToken_MatchingToken_ReturnsTrue()
        {
            var token = RefreshTokenHasher.GenerateRefreshTokenBase64();
            var pepper = TestPepper();
            var hash = RefreshTokenHasher.HashToken(token, pepper);

            Assert.True(RefreshTokenHasher.VerifyToken(token, hash, pepper));
        }

        [Fact]
        public void VerifyToken_NonMatchingToken_ReturnsFalse()
        {
            var token = RefreshTokenHasher.GenerateRefreshTokenBase64();
            var pepper = TestPepper();
            var hash = RefreshTokenHasher.HashToken(token, pepper);

            Assert.False(RefreshTokenHasher.VerifyToken("different", hash, pepper));
        }

        [Fact]
        public void GetPepper_FromEnvironment_ReturnsBytes()
        {
            var pepperBytes = RandomNumberGenerator.GetBytes(32);
            var pepperBase64 = Convert.ToBase64String(pepperBytes);
            Environment.SetEnvironmentVariable("REFRESH_TOKEN_PEPPER", pepperBase64);

            var obtained = RefreshTokenHasher.GetPepper();
            Assert.Equal(pepperBytes, obtained);
        }

        [Fact]
        public void GetPepper_Missing_Throws()
        {
            Environment.SetEnvironmentVariable("REFRESH_TOKEN_PEPPER", null);
            Assert.Throws<InvalidOperationException>(() => RefreshTokenHasher.GetPepper());
        }

        [Fact]
        public void GetPepper_InvalidBase64_Throws()
        {
            Environment.SetEnvironmentVariable("REFRESH_TOKEN_PEPPER", "not-base64!!");
            Assert.Throws<InvalidOperationException>(() => RefreshTokenHasher.GetPepper());
        }
    }
}