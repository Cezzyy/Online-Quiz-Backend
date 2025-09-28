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
        public List<string> Roles { get; set; } = [];
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public List<string> Roles { get; set; } = [];
        
        // Teacher-specific information (optional, only used if Teacher role is assigned)
        public string? Department { get; set; }
        
        // Student-specific information (optional, only used if Student role is assigned)
        public string? StudentNumber { get; set; }
        public int? YearLevel { get; set; }
        public string? Section { get; set; }
        public string? Course { get; set; }
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

    /// <summary>
    /// Clean user summary for login responses (no PII)
    /// </summary>
    public class UserSummaryDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } // Only for non-web clients
        public int ExpiresIn { get; set; } // Seconds until expiration
        public UserSummaryDto User { get; set; } = null!;
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for refresh endpoint - supports both web (cookie) and mobile (body) clients
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>
        /// Optional refresh token for mobile clients. Web clients use cookies instead.
        /// </summary>
        public string? RefreshToken { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } // Only for non-web clients
        public int ExpiresIn { get; set; } // Seconds until expiration
    }

    public class TeacherDto
    {
        public long UserId { get; set; }
        public string? Department { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class StudentDto
    {
        public long UserId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public int? YearLevel { get; set; }
        public string? Section { get; set; }
        public string? Course { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class CreateTeacherDto
    {
        public string? Department { get; set; }
    }

    public class CreateStudentDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public int? YearLevel { get; set; }
        public string? Section { get; set; }
        public string? Course { get; set; }
    }


}