using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class UserRoleModel
    {
        [Key, Column(Order = 0)]
        public long UserId { get; set; }
        
        [Key, Column(Order = 1)]
        public short RoleId { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        [ForeignKey("RoleId")]
        public virtual RoleModel Role { get; set; } = null!;
    }
}