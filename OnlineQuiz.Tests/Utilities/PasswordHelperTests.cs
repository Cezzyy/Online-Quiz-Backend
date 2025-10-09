using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Utilities
{
    public class PasswordHelperTests
    {
        [Fact]
        public void HashPassword_ReturnsDifferentFromOriginal_AndVerifies()
        {
            var password = "P@ssw0rd!";
            var hash = PasswordHelper.HashPassword(password);

            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.NotEqual(password, hash);
            Assert.True(PasswordHelper.VerifyPassword(password, hash));
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ReturnsFalse()
        {
            var password = "CorrectHorseBatteryStaple";
            var hash = PasswordHelper.HashPassword(password);

            Assert.False(PasswordHelper.VerifyPassword("wrong", hash));
        }
    }
}