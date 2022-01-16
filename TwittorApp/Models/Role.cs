using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwittorApp.Models
{
    public class Role
    {
        [Key]
        [Required]
        public int RoleId { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
