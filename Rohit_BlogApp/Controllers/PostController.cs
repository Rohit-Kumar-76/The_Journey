using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rohit_BlogApp.Data;
using Rohit_BlogApp.Models;

namespace Rohit_BlogApp.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext _context;

        public PostController(BlogDbContext context)
        {
            _context = context;
        }

       
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();
            //TempData["SuccessMessage"] = "Post deleted successfully!";

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        public IActionResult Edit(int id, string title, string content)
        {
            var existingPost = _context.Posts.Find(id);
            if (existingPost == null)
            {
                return NotFound();
            }

            // Update only the necessary fields
            existingPost.Title = title;
            existingPost.Content = content;

            _context.SaveChanges();
            TempData["Message"] = "Post updated successfully!";
            return RedirectToAction("Index", "Profile");
        }



        public IActionResult Create()
        {
            var user = HttpContext.Session.GetString("Username");
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please log in to create a post.";
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Create(string title, string content, IFormFile? postImage)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to create a post.";
                return RedirectToAction("Index", "Home");
            }

            // 🚨 Banned Words List
            var bannedWords = new List<string> {
        "sex", "porn", "Chod","Chutiya","bsdk","Bhosdike","gandu","pagal","Lund","Laura","mc","bc","rape", "violence", "kill", "murder", "nude", "abuse", "fuck", "shit", "bitch"
    };

            // 🚦 Check for Banned Words
            bool containsBannedWords(string text) =>
                bannedWords.Any(word => text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

            if (containsBannedWords(title) || containsBannedWords(content))
            {
                TempData["ErrorMessage"] = "Your post contains inappropriate words. Please remove them.";
                return RedirectToAction("Create", "Post");
            }

            string? postImageUrl = null;

            if (postImage != null && postImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(postImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    postImage.CopyTo(fileStream);
                }

                postImageUrl = "/Uploads/" + uniqueFileName;
            }

            var newPost = new Post
            {
                Title = title,
                Content = content,
                PostUrl = postImageUrl,
                UserId = userId.Value
            };

            _context.Posts.Add(newPost);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Post created successfully!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult AddComment(string title, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // List of abusive words (add more as needed)
            var bannedWords = new List<string> {
        "sex", "porn", "chod", "chutiya", "bsdk", "bhosdike", "gandu", "pagal",
        "lund", "laura", "mc", "bc", "rape", "violence", "kill", "murder",
        "nude", "abuse", "fuck", "shit", "bitch"
    };

            // Check for abusive content (case-insensitive)
            foreach (var word in bannedWords)
            {
                if (content.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    TempData["CommentError"] = "Inappropriate language and Abusive words are not allowed.";
                    return RedirectToAction("Post_Detail", new { title = title });
                }
            }

            // Fetch the post by title
            var post = _context.Posts.FirstOrDefault(p => p.Title == title);
            if (post == null)
            {
                //TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction("Index", "Home");
            }

            var comment = new Comment
            {
                PostId = post.Id,
                UserId = userId.Value,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            //TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction("Post_Detail", new { title = title });
        }



        public IActionResult Post_Detail(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return NotFound();
            }

            // Handle '-' as space
            //string decodedTitle = title.Replace("-", " ");

            var post = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefault(p => p.Title == title.Replace("-", " "));

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }


    }
}
