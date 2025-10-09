using Microsoft.Extensions.Options;
using OnlineQuiz.Configuration;
using Xunit;

namespace OnlineQuiz.Tests.Configuration
{
    public class JwtSettingsTests
    {
        [Fact]
        public void DefaultValues_AreSetAsExpected()
        {
            var settings = new JwtSettings();

            Assert.Equal(string.Empty, settings.SecretKey);
            Assert.Equal(string.Empty, settings.Issuer);
            Assert.Equal(string.Empty, settings.Audience);
            Assert.Equal(15, settings.AccessTokenExpirationInMinutes);
            Assert.Equal(7, settings.RefreshTokenExpirationInDays);
        }

        [Fact]
        public void CanAssignCustomValues()
        {
            var settings = new JwtSettings
            {
                SecretKey = "UnitTestSecretKey1234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationInMinutes = 30,
                RefreshTokenExpirationInDays = 14
            };

            Assert.Equal("UnitTestSecretKey1234567890", settings.SecretKey);
            Assert.Equal("TestIssuer", settings.Issuer);
            Assert.Equal("TestAudience", settings.Audience);
            Assert.Equal(30, settings.AccessTokenExpirationInMinutes);
            Assert.Equal(14, settings.RefreshTokenExpirationInDays);
        }

        [Fact]
        public void OptionsCreate_WrapsSettingsInstance()
        {
            var baseSettings = new JwtSettings
            {
                SecretKey = "Key",
                Issuer = "Issuer",
                Audience = "Audience",
                AccessTokenExpirationInMinutes = 60,
                RefreshTokenExpirationInDays = 30
            };

            IOptions<JwtSettings> options = Options.Create(baseSettings);
            var value = options.Value;

            Assert.Equal(baseSettings.SecretKey, value.SecretKey);
            Assert.Equal(baseSettings.Issuer, value.Issuer);
            Assert.Equal(baseSettings.Audience, value.Audience);
            Assert.Equal(baseSettings.AccessTokenExpirationInMinutes, value.AccessTokenExpirationInMinutes);
            Assert.Equal(baseSettings.RefreshTokenExpirationInDays, value.RefreshTokenExpirationInDays);
        }
    }
}