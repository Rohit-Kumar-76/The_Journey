using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rohit_BlogApp.Data;
using Rohit_BlogApp.Models;

namespace Rohit_BlogApp.Controllers
{
    [SessionCheck]
    public class HomeController : Controller
    {
        private readonly BlogDbContext _context;

        public HomeController(BlogDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var posts = _context.Posts
                                .Include(p => p.User)  // Include User to avoid null reference
                                .OrderByDescending(p => p.CreatedAt)
                                .ToList();
          

            return View(posts);
        }



        public IActionResult Search(string username)
        {
            var users = _context.Users
                        .Where(u => u.Username.ToLower().Contains(username.ToLower()))
                        .ToList();

            if (users.Any())
            {
                // Specify path to Profile folder
                return View("/Views/Profile/UserList.cshtml", users);
            }
            return Redirect("/Profile/NoUser");
        }



        [HttpPost]
        public IActionResult Like(string title)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { error = "User not logged in." });
            }

            var post = _context.Posts.FirstOrDefault(p => p.Title == title);
            if (post == null)
            {
                return Json(new { error = "Post not found." });
            }

            var existingLike = _context.Likes.FirstOrDefault(l => l.PostId == post.Id && l.UserId == userId);

            if (existingLike == null)
            {
                _context.Likes.Add(new Like { PostId = post.Id, UserId = userId.Value });
            }
            else
            {
                _context.Likes.Remove(existingLike);
            }

            _context.SaveChanges();

            var likeCount = _context.Likes.Count(l => l.PostId == post.Id);
            return Json(new { likes = likeCount });
        }





        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
