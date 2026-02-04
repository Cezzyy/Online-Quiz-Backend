using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class StudentModel
    {
        [Key]
        public long UserId { get; set; }
        
        [Required]
        [StringLength(25)]
        public string StudentNumber { get; set; } = string.Empty;
        
        [Column("Year_Level")]
        public int? YearLevel { get; set; }
        
        [StringLength(60)]
        public string? Section { get; set; }
        
        [StringLength(120)]
        public string? Course { get; set; }
        
        // ACLC Academic Information
        [StringLength(100)]
        public string? Program { get; set; }
        
        [StringLength(20)]
        public string? YearLevelString { get; set; }
        
        // Enrollment Status
        [StringLength(20)]
        public string EnrollmentStatus { get; set; } = "Active";
        
        // Guardian/Parent Information
        [StringLength(100)]
        public string? GuardianName { get; set; }
        
        [StringLength(30)]
        public string? GuardianContactNumber { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
    }
}