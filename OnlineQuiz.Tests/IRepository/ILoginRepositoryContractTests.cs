using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using Xunit;

namespace OnlineQuiz.Tests.IRepository
{
    // Stub implementation to validate interface contract and return types
    internal class StubLoginRepository : ILoginRepository
    {
        public Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto)
        {
            var response = new ServiceResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Authenticated",
                Data = new LoginResponseDto { AccessToken = "token", TokenType = "Bearer", ExpiresIn = 3600 }
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<UserModel>> ValidateUserCredentialsAsync(string email, string password)
        {
            var response = new ServiceResponse<UserModel>
            {
                Success = true,
                Message = "Valid",
                Data = new UserModel { Email = email, FullName = "Stub User" }
            };
            return Task.FromResult(response);
        }

        public Task<ServiceResponse> LogoutAsync(long userId)
        {
            var response = new ServiceResponse("Logged out")
            {
                Success = false // constructor sets success=false when message provided
            };
            return Task.FromResult(response);
        }
    }

    public class ILoginRepositoryContractTests
    {
        [Fact]
        public void StubLoginRepository_Implements_ILoginRepository()
        {
            Assert.Contains(typeof(ILoginRepository), typeof(StubLoginRepository).GetInterfaces());
        }

        [Fact]
        public async Task AuthenticateAsync_Returns_ServiceResponse_With_LoginResponseDto()
        {
            ILoginRepository repo = new StubLoginRepository();
            var result = await repo.AuthenticateAsync(new LoginDto { Email = "a@b.com", Password = "pass" });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.IsType<ServiceResponse<LoginResponseDto>>(result);
            Assert.NotNull(result.Data);
            Assert.Equal("Bearer", result.Data!.TokenType);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_Returns_ServiceResponse_With_UserModel()
        {
            ILoginRepository repo = new StubLoginRepository();
            var result = await repo.ValidateUserCredentialsAsync("user@example.com", "secret");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.IsType<ServiceResponse<UserModel>>(result);
            Assert.NotNull(result.Data);
            Assert.Equal("user@example.com", result.Data!.Email);
        }

        [Fact]
        public async Task LogoutAsync_Returns_NonGeneric_ServiceResponse()
        {
            ILoginRepository repo = new StubLoginRepository();
            var result = await repo.LogoutAsync(1);

            Assert.NotNull(result);
            Assert.IsType<ServiceResponse>(result);
            Assert.False(result.Success);
            Assert.Equal("Logged out", result.Message);
        }
    }
}