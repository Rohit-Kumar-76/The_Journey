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

      
    }
}
