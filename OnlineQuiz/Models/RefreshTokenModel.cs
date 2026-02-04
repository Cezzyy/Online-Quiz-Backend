using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    [Table("RefreshTokens")]
    public class RefreshTokenModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public UserModel User { get; set; } = null!;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        // Token Metadata
        [StringLength(100)]
        public string? DeviceName { get; set; }
        
        // Revocation Information
        public long? RevokedBy { get; set; }
        
        [StringLength(200)]
        public string? RevokedReason { get; set; }
        
        // Timestamps
        public DateTime? LastUsedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsActive => !IsExpired && !IsRevoked;
    }
}