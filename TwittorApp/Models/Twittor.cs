using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwittorApp.Models
{
    public class Twittor
    {
        [Key]
        [Required]
        public int TwittorId { get; set; }
        [Required]
        public string TwittorContent { get; set; }
        [Required]
        public DateTime TwittorCreated { get; set; }
        [Required]
        public int UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
