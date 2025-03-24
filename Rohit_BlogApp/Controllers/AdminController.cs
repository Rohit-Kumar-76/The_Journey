using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rohit_BlogApp.Data;
using System.Linq;
using Rohit_BlogApp.Models;

namespace Rohit_BlogApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly BlogDbContext _context;

        public AdminController(BlogDbContext context)
        {
            _context = context;
        }

        // Admin Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Username == username && a.Password == password);
            if (admin != null)
            {
                HttpContext.Session.SetString("Admin", admin.Username);
                HttpContext.Session.SetString("Name", admin.Name);
                return RedirectToAction("Dash", "Admin");
            }
            TempData["ErrorMessage"] = "Invalid credentials!";
            return View();
        }


        public IActionResult User_Details(int id)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }
            var user = _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        public IActionResult Post_Details(int id)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }
            var post = _context.Posts
                .Include(p => p.User)  // Includes the user who created the post
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)  // Includes the user for each comment
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }


        // Admin Dashboard - Show Users and Posts
        public IActionResult Dash(int? userId)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var users = _context.Users.ToList();
            var posts = userId.HasValue ? _context.Posts.Where(p => p.UserId == userId).ToList() : _context.Posts.ToList();

            var viewModel = new UserPostViewModel
            {
                Users = users,
                Posts = posts
            };

            return View(viewModel);
        }

        // User Management
        public IActionResult Users(string search)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }
            var users = _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Username.Contains(search) || u.Email.Contains(search));
            }

            return View(users.ToList());
        }


        public IActionResult Posts(string search)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Where(p => string.IsNullOrEmpty(search) || p.Title.Contains(search))
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(posts);
        }



        public IActionResult DeleteUser(int id)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Users");
        }

       
        [HttpPost]
        public IActionResult DeletePost(int id)
        {

            var post = _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Likes)  // Include likes
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Delete related likes first
            if (post.Likes != null && post.Likes.Any())
            {
                _context.Likes.RemoveRange(post.Likes);
            }

            // Delete related comments
            if (post.Comments != null && post.Comments.Any())
            {
                _context.Comments.RemoveRange(post.Comments);
            }

            // Finally, delete the post
            _context.Posts.Remove(post);
            _context.SaveChanges();

            return RedirectToAction("Posts");
        }



        // Change Password
        public IActionResult Change_Pass()
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Change_Pass(string oldPassword, string newPassword)
        {
            var adminUsername = HttpContext.Session.GetString("Admin");
            var admin = _context.Admins.FirstOrDefault(a => a.Username == adminUsername && a.Password == oldPassword);
            if (admin != null)
            {
                admin.Password = newPassword;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Dash");
            }
            TempData["ErrorMessage"] = "Old password is incorrect.";
            return View();
        }

        // Admin Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
