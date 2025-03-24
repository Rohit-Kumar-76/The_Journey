namespace Rohit_BlogApp.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public List<Post> Posts { get; set; }
        public bool IsOwnProfile { get; set; }
    }
}
