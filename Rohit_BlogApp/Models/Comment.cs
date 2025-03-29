using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Rohit_BlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key for User (Comment Author)
        public int UserId { get; set; }
        public User User { get; set; }

        // Foreign Key for Post
        public int PostId { get; set; }
        public Post Post { get; set; }

        // Parent Comment for Reply System (null if it's a main comment)
        public int? ParentCommentId { get; set; }
        public Comment ParentComment { get; set; }

        // List of Replies
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
