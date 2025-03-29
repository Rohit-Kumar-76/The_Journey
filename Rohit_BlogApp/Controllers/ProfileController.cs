using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rohit_BlogApp.Data;
using Rohit_BlogApp.Models;

namespace Rohit_BlogApp.Controllers
{
    [SessionCheck]
    public class ProfileController : Controller
    {
        private readonly BlogDbContext _context;

        public ProfileController(BlogDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users
                .Include(u => u.Posts)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        public IActionResult UserList()
        {
            return View();
        }

        [Route("Profile/UserProfile/{username}")]
        public IActionResult UserProfile(string username)
        {
            var currentUser = HttpContext.Session.GetString("Username");
            var user = _context.Users.Include(u => u.Posts)
                                     .FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            bool isOwnProfile = currentUser != null && currentUser == user.Username;

            var model = new ProfileViewModel
            {
                User = user,
                Posts = user.Posts.OrderByDescending(p => p.CreatedAt).ToList(),
                IsOwnProfile = isOwnProfile
            };

            return View(model);
        }

        public IActionResult NoUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string username, string userBio, IFormFile? profileImage)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to update profile.";
                return RedirectToAction("Login", "Auth");
            }

            // Existing user ko fetch karte hain
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (existingUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Profile");
            }

            // 🛡️ Abusive Words List
            List<string> abusiveWords = new List<string> {
                     "sex", "porn", "Chod","Chutiya","bsdk","Bhosdike","gandu","pagal","Lund","Laura","mc","bc","rape", "violence", "kill", "murder", "nude", "abuse", "fuck", "shit", "bitch"
    };          // Apne words add karo

            // 🚦 Username Validation: No spaces, no capitals, cannot start with numbers or special characters (except underscore)
            if (!string.IsNullOrEmpty(username))
            {
                var usernamePattern = @"^[a-z_](?:[a-z0-9_]|(?<!\.)\.)*$";

                if (!Regex.IsMatch(username, usernamePattern))
                {
                    TempData["ErrorMessage"] = "Invalid username. ";
                    return RedirectToAction("Index", "Profile");
                }

                // 🚫 Abusive Check in Username
                if (abusiveWords.Any(word => username.ToLower().Contains(word)))
                {
                    TempData["ErrorMessage"] = "Inappropriate content detected in username!";
                    return RedirectToAction("Index", "Profile");
                }

                existingUser.Username = username;
            }

            // 🚫 Abusive Check in Bio
            if (!string.IsNullOrEmpty(userBio))
            {
                if (abusiveWords.Any(word => userBio.ToLower().Contains(word)))
                {
                    TempData["ErrorMessage"] = "Inappropriate content detected in bio!";
                    return RedirectToAction("Index", "Profile");
                }
                existingUser.UserBio = userBio;
            }

            // 🖼️ Profile Image Upload
            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserProfileImg");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(fileStream);
                }

                existingUser.ProfileUrl = "/UserProfileImg/" + uniqueFileName;
            }

            _context.Users.Update(existingUser);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index", "Profile");
        }

    }
}
