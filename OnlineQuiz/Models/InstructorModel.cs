using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    [Table("Instructors")]
    public class InstructorModel
    {
        [Key]
        public long UserId { get; set; }
        
        [StringLength(120)]
        public string? Department { get; set; }
        
        // Professional Information
        [StringLength(100)]
        public string? Title { get; set; } // Prof., Engr., Mr., Ms., etc.
        
        [StringLength(100)]
        public string? Specialization { get; set; }
        
        // ACLC Office Information
        [StringLength(100)]
        public string? OfficeLocation { get; set; }
        
        [StringLength(30)]
        public string? OfficePhone { get; set; }
        
        // Availability Schedule
        [StringLength(500)]
        public string? ConsultationHours { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        public virtual ICollection<CourseModel> Courses { get; set; } = new List<CourseModel>();
    }
}
