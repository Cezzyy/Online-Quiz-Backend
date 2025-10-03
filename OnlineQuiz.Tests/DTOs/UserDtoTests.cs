using OnlineQuiz.DTOs;
using Xunit;

namespace OnlineQuiz.Tests.DTOs
{
    public class UserDtoTests
    {
        [Fact]
        public void UserDto_Defaults_AreInitialized()
        {
            var dto = new UserDto();
            Assert.Equal(0, dto.UserId);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.Equal(string.Empty, dto.Status);
            Assert.NotNull(dto.Roles);
            Assert.Empty(dto.Roles);
        }

        [Fact]
        public void CreateUserDto_Defaults_AreInitialized()
        {
            var dto = new CreateUserDto();
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.Password);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.NotNull(dto.Roles);
            Assert.Empty(dto.Roles);
        }

        [Fact]
        public void UpdateUserDto_Assignments_Work()
        {
            var dto = new UpdateUserDto
            {
                FullName = "Updated",
                ContactNumber = "123",
                EmergencyContactNumber = "456",
                Status = "Active"
            };

            Assert.Equal("Updated", dto.FullName);
            Assert.Equal("123", dto.ContactNumber);
            Assert.Equal("456", dto.EmergencyContactNumber);
            Assert.Equal("Active", dto.Status);
        }

        [Fact]
        public void UserSummaryDto_Defaults_AreInitialized()
        {
            var dto = new UserSummaryDto();
            Assert.Equal(0, dto.Id);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.NotNull(dto.Roles);
            Assert.Empty(dto.Roles);
        }

        [Fact]
        public void LoginResponseDto_Defaults_AreInitialized()
        {
            var dto = new LoginResponseDto();
            Assert.Equal(string.Empty, dto.AccessToken);
            Assert.Equal("Bearer", dto.TokenType);
            Assert.Equal(0, dto.ExpiresIn);
            // Property is null at runtime by default (null-suppressed in model)
            Assert.Null(dto.User);
            // After assignment, it should be non-null and of correct type
            dto.User = new UserSummaryDto();
            Assert.NotNull(dto.User);
            Assert.IsType<UserSummaryDto>(dto.User);
        }

        [Fact]
        public void RefreshTokenDto_Defaults_AreInitialized()
        {
            var dto = new RefreshTokenDto();
            Assert.Equal(string.Empty, dto.RefreshToken);
        }

        [Fact]
        public void RefreshRequest_Assignment_Works()
        {
            var dto = new RefreshRequest { RefreshToken = "abc" };
            Assert.Equal("abc", dto.RefreshToken);
        }

        [Fact]
        public void RefreshTokenResponseDto_Defaults_AreInitialized()
        {
            var dto = new RefreshTokenResponseDto();
            Assert.Equal(string.Empty, dto.AccessToken);
            Assert.Equal("Bearer", dto.TokenType);
            Assert.Equal(0, dto.ExpiresIn);
        }

        [Fact]
        public void TeacherDto_Assignment_Works()
        {
            var dto = new TeacherDto
            {
                UserId = 1,
                Department = "CS",
                User = new UserDto { UserId = 1, Email = "a@b.com", FullName = "A B" }
            };

            Assert.Equal(1, dto.UserId);
            Assert.Equal("CS", dto.Department);
            Assert.NotNull(dto.User);
            Assert.Equal("a@b.com", dto.User.Email);
        }

        [Fact]
        public void StudentDto_Assignment_Works()
        {
            var dto = new StudentDto
            {
                UserId = 2,
                StudentNumber = "SN-001",
                YearLevel = 2,
                Section = "A",
                Course = "BSCS",
                User = new UserDto { UserId = 2, Email = "c@d.com", FullName = "C D" }
            };

            Assert.Equal(2, dto.UserId);
            Assert.Equal("SN-001", dto.StudentNumber);
            Assert.Equal(2, dto.YearLevel);
            Assert.Equal("A", dto.Section);
            Assert.Equal("BSCS", dto.Course);
            Assert.Equal("c@d.com", dto.User.Email);
        }
    }
}