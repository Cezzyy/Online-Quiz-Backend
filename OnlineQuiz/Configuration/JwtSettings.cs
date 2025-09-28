namespace OnlineQuiz.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationInMinutes { get; set; } = 15; // Short-lived access token
        public int RefreshTokenExpirationInDays { get; set; } = 7; // Longer-lived refresh token
    }
}