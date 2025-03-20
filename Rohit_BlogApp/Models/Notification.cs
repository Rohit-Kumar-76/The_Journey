using System.ComponentModel.DataAnnotations;

namespace Rohit_BlogApp.Models
{
    public class Notification
    {

        public int Id { get; set; }

        // The user who receives the notification
        public int UserId { get; set; }
        public User User { get; set; }

        // The post related to the notification
        public int PostId { get; set; }
        public Post Post { get; set; }

        // Type of notification: "Like" or "Comment"
        [Required]
        public string Type { get; set; }

        // To track whether the notification has been read
        public bool IsRead { get; set; } = false;

        // Timestamp of the notification
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

