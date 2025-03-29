using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rohit_BlogApp.Data;
using Rohit_BlogApp.Models;
using Rohit_BlogApp.ViewModels;

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
        public IActionResult AddComment(string title, string content, int? parentCommentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Fetch the post by title
            var post = _context.Posts.FirstOrDefault(p => p.Title == title);
            if (post == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Add Comment or Reply
            var comment = new Comment
            {
                PostId = post.Id,
                UserId = userId.Value,
                Content = content,
                ParentCommentId = parentCommentId,  // If it's a reply, this will not be null
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            // === Add Notification for Post Owner or Parent Comment Owner ===
            if (parentCommentId == null)  // If it's a normal comment
            {
                if (post.UserId != userId)  // Apne post pe khud comment karne pe notification na aaye
                {
                    var notification = new Notification
                    {
                        UserId = post.UserId,  // Post Owner ID
                        PostId = post.Id,
                        Type = "Comment",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };

                    _context.Notifications.Add(notification);
                    _context.SaveChanges();
                }
            }
            else  // If it's a reply
            {
                var parentComment = _context.Comments.Find(parentCommentId);
                if (parentComment != null && parentComment.UserId != userId)
                {
                    var notification = new Notification
                    {
                        UserId = parentComment.UserId,  // Comment Owner ID
                        PostId = post.Id,
                        Type = "Reply",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };

                    _context.Notifications.Add(notification);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Post_Detail", new { title = title });
        }



        [HttpPost]
        public IActionResult AddReply(string title, string content, int parentCommentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var parentComment = _context.Comments
                .Include(c => c.Post) // Ensure Post data is loaded
                .FirstOrDefault(c => c.Id == parentCommentId);

            if (parentComment == null)
            {
                return RedirectToAction("Post_Detail", new { title = title });
            }

            var reply = new Comment
            {
                PostId = parentComment.PostId,
                UserId = userId.Value,
                Content = content,
                CreatedAt = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(reply);
            _context.SaveChanges();

            // === Add Notification for Parent Comment Owner ===
            if (parentComment.UserId != userId.Value)  // Apni reply pe notify na ho
            {
                var notification = new Notification
                {
                    UserId = parentComment.UserId,  // Parent Comment Owner ID
                    PostId = parentComment.PostId,
                    Type = "Reply",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("Post_Detail", new { title = title });
        }




        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                TempData["DeleteMessage"] = "Comment not found!";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Get User ID from Session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["DeleteMessage"] = "Unauthorized action!";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            var post = await _context.Posts.FindAsync(comment.PostId);
            if (post == null)
            {
                TempData["DeleteMessage"] = "Post not found!";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Only comment owner or post owner can delete
            if (userId != comment.UserId && userId != post.UserId)
            {
                TempData["DeleteMessage"] = "You are not allowed to delete this comment!";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Delete comment and replies
            //await DeleteReplies(comment.Id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["DeleteMessage"] = "Comment deleted successfully!";
            return Redirect(Request.Headers["Referer"].ToString()); // Redirect to the same page
        }


        public IActionResult GetNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = _context.Notifications
                  .Include(n => n.Post)
                  .Include(n => n.User)  // Post Owner ke liye
                  .Include(n => n.Post.Comments)  // Comments load karne ke liye
                      .ThenInclude(c => c.User)  // Comment karne wale ka User data
                  .Where(n => n.UserId == userId)
                  .OrderByDescending(n => n.CreatedAt)
                  .ToList();



            return PartialView("_NotificationsPartial", notifications);
        }

        public IActionResult MarkNotificationsAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
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
