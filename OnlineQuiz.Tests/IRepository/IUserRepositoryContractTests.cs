using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models.Response;
using Xunit;

namespace OnlineQuiz.Tests.IRepository
{
    // Stub implementation to validate interface contract and return types
    internal class StubUserRepository : IUserRepository
    {
        public Task<ServiceResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            var response = new ServiceResponse<UserDto>
            {
                Success = true,
                Message = "Created",
                Data = new UserDto { UserId = 1, Email = createUserDto.Email, FullName = createUserDto.FullName }
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse> DeleteUserAsync(long userId)
        {
            var response = new ServiceResponse("User not found");
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>
            {
                Success = true,
                Message = "OK",
                Data = Array.Empty<UserDto>()
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<StudentDto>>> GetAllStudentsWithProfileAsync()
        {
            var response = new ServiceResponse<IEnumerable<StudentDto>>
            {
                Success = true,
                Data = Array.Empty<StudentDto>()
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<TeacherDto>>> GetAllTeachersWithProfileAsync()
        {
            var response = new ServiceResponse<IEnumerable<TeacherDto>>
            {
                Success = true,
                Data = Array.Empty<TeacherDto>()
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email)
        {
            var response = ServiceResponse<UserDto>.Fail("User not found");
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<UserDto>> GetUserByIdAsync(long userId)
        {
            var response = ServiceResponse<UserDto>.Fail("User not found");
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(string roleName)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>(Array.Empty<UserDto>())
            {
                Message = "OK"
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<UserDto>> UpdateUserAsync(long userId, UpdateUserDto updateUserDto)
        {
            var response = new ServiceResponse<UserDto>
            {
                Success = true,
                Message = "Updated",
                Data = new UserDto { UserId = userId, Email = "updated@example.com", FullName = updateUserDto.FullName }
            };
            return Task.FromResult(response);
        }
    }

    public class IUserRepositoryContractTests
    {
        [Fact]
        public void StubUserRepository_Implements_IUserRepository()
        {
            Assert.Contains(typeof(IUserRepository), typeof(StubUserRepository).GetInterfaces());
        }

        [Fact]
        public async Task GetAllUsersAsync_Returns_ServiceResponse_Enumerable_UserDto()
        {
            IUserRepository repo = new StubUserRepository();
            var result = await repo.GetAllUsersAsync();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.IsType<ServiceResponse<IEnumerable<UserDto>>>(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task CreateUserAsync_Returns_ServiceResponse_UserDto()
        {
            IUserRepository repo = new StubUserRepository();
            var result = await repo.CreateUserAsync(new CreateUserDto { Email = "new@example.com", Password = "Pass123!", FullName = "New User", Roles = new List<string>() });
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.IsType<ServiceResponse<UserDto>>(result);
            Assert.NotNull(result.Data);
            Assert.Equal("new@example.com", result.Data!.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_Returns_ServiceResponse_UserDto()
        {
            IUserRepository repo = new StubUserRepository();
            var result = await repo.UpdateUserAsync(2, new UpdateUserDto { FullName = "Updated Name" });
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.IsType<ServiceResponse<UserDto>>(result);
            Assert.Equal(2, result.Data!.UserId);
            Assert.Equal("Updated Name", result.Data!.FullName);
        }

        [Fact]
        public async Task DeleteUserAsync_Returns_ServiceResponse_Fail()
        {
            IUserRepository repo = new StubUserRepository();
            var result = await repo.DeleteUserAsync(99);
            Assert.NotNull(result);
            Assert.IsType<ServiceResponse>(result);
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }
    }
}