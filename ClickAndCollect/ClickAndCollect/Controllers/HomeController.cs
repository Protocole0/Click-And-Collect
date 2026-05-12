using System.Diagnostics;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Veuillez entrer à la fois le nom d'utilisateur et le mot de passe.";
                return View();
            }
            else if (email == "admin" && password == "password")
            {
                return RedirectToAction("Index", "Employee");
            }
            else
            {
                ViewBag.ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect.";
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
