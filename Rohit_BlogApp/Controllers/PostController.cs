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
            TempData["SuccessMessage"] = "Post deleted successfully!";

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
        public IActionResult Edit(Post post)
        {
            if (ModelState.IsValid)
            {

                var existingPost = _context.Posts.Find(post.Id);
                if (existingPost != null)
                {
                    // Only update the fields you want to edit
                    existingPost.Title = post.Title;
                    existingPost.Content = post.Content;

                    // Keep existing data untouched
                    existingPost.User = existingPost.User;
                    existingPost.Likes = existingPost.Likes;
                    existingPost.Comments = existingPost.Comments;

                    _context.SaveChanges();
                    TempData["Message"] = "Post updated successfully!";
                    return RedirectToAction("Profile", "Post");
                }
            }
            return View(post);
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
        public IActionResult AddComment(int postId, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId.Value,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Post_Detail", new { id = postId });
        }


        [HttpPost]
        public IActionResult Like(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));  // Get logged-in user ID

            var existingLike = _context.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                // Add a new like
                var newLike = new Like { PostId = postId, UserId = userId };
                _context.Likes.Add(newLike);
            }
            else
            {
                // Remove the existing like (unlike)
                _context.Likes.Remove(existingLike);
            }

            _context.SaveChanges();

            // Return updated like count
            var likeCount = _context.Likes.Count(l => l.PostId == postId);
            return Json(new { likes = likeCount });
        }



        public IActionResult Post_Detail(int id)
        {
            var post = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)  // Ensure User is loaded with Comments
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
    }
}
