using Microsoft.AspNetCore.Mvc;

namespace Rohit_BlogApp.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
