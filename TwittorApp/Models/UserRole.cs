using System.ComponentModel.DataAnnotations;

namespace TwittorApp.Models
{
    public class UserRole
    {
        [Key]
        [Required]
        public int UserRoleId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
