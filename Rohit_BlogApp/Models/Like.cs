namespace Rohit_BlogApp.Models
{
    public class Like
    {
        public int Id { get; set; }

        // Foreign Key for User
        public int UserId { get; set; }
        public User User { get; set; }

        // Foreign Key for Post
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
