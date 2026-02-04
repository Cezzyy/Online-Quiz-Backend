using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class ExportImportLogModel
    {
        [Key]
        public long LogId { get; set; }
        
        [Required]
        public long UserId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Action { get; set; } = string.Empty; // Export, Import
        
        [Required]
        [StringLength(60)]
        public string Entity { get; set; } = string.Empty;
        
        [StringLength(255)]
        [Column("File_Name")]
        public string? FileName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // File Information
        public long? FileSize { get; set; }
        
        [StringLength(50)]
        public string? FileType { get; set; }
        
        // Processing Status
        [StringLength(20)]
        public string Status { get; set; } = "Completed";
        
        public int? RecordsProcessed { get; set; }
        
        public int? RecordsSucceeded { get; set; }
        
        public int? RecordsFailed { get; set; }
        
        // Error Information
        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorLog { get; set; }
        
        // Processing Time
        public int? ProcessingTimeMs { get; set; }
        
        // Timestamps
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
    }
}