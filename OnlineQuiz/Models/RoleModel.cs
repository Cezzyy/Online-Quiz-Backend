using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.Models
{
    public class RoleModel
    {
        [Key]
        public short RoleId { get; set; }
        
        [Required]
        [StringLength(30)]
        public string Name { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();
    }
}