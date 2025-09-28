using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class NotificationModel
    {
        [Key]
        public long NotificationId { get; set; }
        
        [Required]
        public long UserId { get; set; }
        
        public long? CourseId { get; set; }
        
        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string Type { get; set; } = string.Empty;
        
        [Column("Is_Read")]
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        [ForeignKey("CourseId")]
        public virtual CourseModel? Course { get; set; }
    }
}