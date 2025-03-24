namespace Rohit_BlogApp.Models
{
    public class HomeViewModel
    {
        public User User { get; set; }  // User Information
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
