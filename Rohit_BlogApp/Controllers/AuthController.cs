using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rohit_BlogApp.Data;
using Rohit_BlogApp.Models;

namespace Rohit_BlogApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly BlogDbContext _context;
        public AuthController(BlogDbContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(a => a.Email == email && a.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Email", user.Email);
                // Store User ID as Integer in Session
                HttpContext.Session.SetInt32("UserId", user.Id);

                HttpContext.Session.SetString("ProfileUrl", user?.ProfileUrl ?? "/Img/profile-icon.jpg");


                return RedirectToAction("Index", "Home");  // Fix Redirect
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid credentials. Please try again.";
            }
            return View();
        }

         public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            try
            {
                // 🚦 Username Validation: No spaces, no capitals, cannot start with numbers or special characters (except underscore)
                var usernamePattern = @"^[a-z_][a-z0-9_]*$";
                if (!Regex.IsMatch(username, usernamePattern))
                {
                    TempData["ErrorMessage"] = "Invalid username. Use only lowercase letters, numbers, and underscores. Must start with a letter or underscore.";
                    return View();
                }

                // Check if user already exists
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email already registered. Please use another email.";
                    return View();
                }

                // Create new user
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "User registered successfully!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = "Something went wrong! Please try again.";
            }

            return View();
        }





    }
}
