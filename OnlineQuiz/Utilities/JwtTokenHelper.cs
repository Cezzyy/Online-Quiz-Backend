using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using OnlineQuiz.Configuration;
using OnlineQuiz.Models;

namespace OnlineQuiz.Utilities
{
    public static class JwtTokenHelper
    {
        /// <summary>
        /// Generates a JWT token for the given user
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="roles">User roles</param>
        /// <param name="jwtSettings">JWT configuration settings</param>
        /// <returns>JWT token string</returns>
        public static string GenerateToken(UserModel user, IEnumerable<string> roles, JwtSettings jwtSettings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var rawKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey ?? string.Empty);
            // Ensure consistent key length (at least 256 bits) for HMAC-SHA256
            var keyBytes = rawKey.Length < 32 ? SHA256.HashData(rawKey) : rawKey;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("status", user.Status)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationInMinutes),
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a secure refresh token
        /// </summary>
        /// <returns>Base64 encoded refresh token</returns>
        public static string GenerateRefreshToken()
        {
            // Use the new secure token generator
            return RefreshTokenHasher.GenerateRefreshTokenBase64();
        }

        /// <summary>
        /// Validates a JWT token and returns the claims principal
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <param name="jwtSettings">JWT configuration settings</param>
        /// <returns>Claims principal if valid, null otherwise</returns>
        public static ClaimsPrincipal? ValidateToken(string token, JwtSettings jwtSettings)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var rawKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey ?? string.Empty);
                var keyBytes = rawKey.Length < 32 ? SHA256.HashData(rawKey) : rawKey;

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}