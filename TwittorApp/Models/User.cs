using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwittorApp.Models
{
    public class User
    {
        [Key]
        [Required]
        public int UserId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool IsBanned { get; set; }
        [Required]
        public DateTime UserCreated { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<Twittor> Twittors { get; set; }
    }
}
