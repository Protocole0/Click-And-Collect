using ClickAndCollect.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode == 404)
            {
                string referer = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }

                if (HttpContext.Session.GetInt32("user_id").HasValue)
                {
                    if (HttpContext.Session.GetString("user_type") == "client")
                        return RedirectToAction("Index");
                    return RedirectToAction("Dashboard", "Employee");
                }

                return RedirectToAction("Index", "Home");
            }

            return View("Error");
        }
    }
}
