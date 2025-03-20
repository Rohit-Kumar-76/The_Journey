using System.ComponentModel.DataAnnotations;

namespace Rohit_BlogApp.Models
{
    // Comment Model
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key for User
        public int UserId { get; set; }
        public User User { get; set; }

        // Foreign Key for Post
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
