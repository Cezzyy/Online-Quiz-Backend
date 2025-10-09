using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Utilities
{
    // Precompute a low-cost hash outside individual test method timing
    public class PasswordHashFixture
    {
        public string Password => "P@ssw0rd!";
        public string LowCostHash { get; }

        public PasswordHashFixture()
        {
            const int WorkFactor = 4; // keep test-time work small
            LowCostHash = BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.BCrypt.GenerateSalt(WorkFactor));
        }
    }

    public class PasswordHelperTests : IClassFixture<PasswordHashFixture>
    {
        private readonly PasswordHashFixture _fixture;

        public PasswordHelperTests(PasswordHashFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            Assert.True(PasswordHelper.VerifyPassword(_fixture.Password, _fixture.LowCostHash));
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            Assert.False(PasswordHelper.VerifyPassword("wrong", _fixture.LowCostHash));
        }
    }
}