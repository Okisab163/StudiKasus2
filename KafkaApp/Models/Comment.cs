using System;
using System.Collections.Generic;

#nullable disable

namespace KafkaApp.Models
{
    public partial class Comment
    {
        public int CommentId { get; set; }
        public string CommentContent { get; set; }
        public DateTime CommentCreated { get; set; }
        public int TwittorId { get; set; }

        public virtual Twittor Twittor { get; set; }
    }
}
