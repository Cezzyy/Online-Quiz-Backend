namespace OnlineQuiz.DTOs
{
    public class UserDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? ContactNumber { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? Status { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }


}