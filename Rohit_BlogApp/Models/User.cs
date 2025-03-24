using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Rohit_BlogApp.Models
{
    // User Model
    public class User
    {

        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        public string Username { get; set; }
       
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? ProfileUrl { get; set; }  
        public string? UserBio {  get; set; }   
        [Required]
        public string Password { get; set; }

   
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}



