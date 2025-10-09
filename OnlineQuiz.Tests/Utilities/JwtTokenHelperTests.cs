using System;
using System.Linq;
using System.Security.Claims;
using OnlineQuiz.Configuration;
using OnlineQuiz.Models;
using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Utilities
{
    public class JwtTokenHelperTests
    {
        private static JwtSettings CreateSettings(string? secretOverride = null)
        {
            return new JwtSettings
            {
                SecretKey = secretOverride ?? "super-secret-key-for-tests-1234567890-0987654321",
                Issuer = "OnlineQuizAPI",
                Audience = "OnlineQuizUsers",
                AccessTokenExpirationInMinutes = 10,
                RefreshTokenExpirationInDays = 7
            };
        }

        private static UserModel CreateUser()
        {
            return new UserModel
            {
                UserId = 123,
                Email = "user@example.com",
                FullName = "Test User",
                Status = "Active",
                PasswordHash = "placeholder"
            };
        }

        [Fact]
        public void GenerateToken_WithValidSettings_ReturnsNonEmptyToken()
        {
            var settings = CreateSettings();
            var user = CreateUser();
            var roles = new[] { "Admin", "User" };

            var token = JwtTokenHelper.GenerateToken(user, roles, settings);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void ValidateToken_ReturnsPrincipal_WithExpectedClaims()
        {
            var settings = CreateSettings();
            var user = CreateUser();
            var roles = new[] { "Admin", "User" };
            var token = JwtTokenHelper.GenerateToken(user, roles, settings);

            var principal = JwtTokenHelper.ValidateToken(token, settings);
            Assert.NotNull(principal);

            var claims = principal!.Claims.ToList();
            Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.UserId.ToString());
            Assert.Contains(claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(claims, c => c.Type == ClaimTypes.Name && c.Value == user.FullName);
            Assert.Contains(claims, c => c.Type == "status" && c.Value == user.Status);
            Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ReturnsNull()
        {
            var settings = CreateSettings();
            var principal = JwtTokenHelper.ValidateToken("invalid.token.string", settings);
            Assert.Null(principal);
        }

        [Fact]
        public void GenerateToken_WithShortSecret_ThrowsDueToWeakKey()
        {
            var settings = CreateSettings(secretOverride: "short");
            var user = CreateUser();
            var roles = Array.Empty<string>();

            Assert.ThrowsAny<Exception>(() => JwtTokenHelper.GenerateToken(user, roles, settings));
        }
    }
}