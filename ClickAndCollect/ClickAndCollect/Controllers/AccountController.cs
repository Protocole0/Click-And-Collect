using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using ClickAndCollect.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class AccountController : Controller
    {
        private readonly IClientDAL _clientDAL;

        public const string SessionKeyId        = "client_id";
        public const string SessionKeyFirstname = "client_firstname";
        public const string SessionKeyLastname  = "client_lastname";
        public const string SessionKeyUserType  = "user_type";

        public AccountController(IClientDAL clientDAL)
        {
            _clientDAL = clientDAL;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (IsLoggedIn(HttpContext.Session))
                return RedirectBasedOnRole(HttpContext.Session);
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Client? client = await _clientDAL.GetByEmailAndPasswordAsync(model.Email, model.Password);
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return View(model);
            }

            StoreClientInSession(client);
            return RedirectBasedOnRole(HttpContext.Session);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            if (IsLoggedIn(HttpContext.Session))
                return RedirectBasedOnRole(HttpContext.Session);
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _clientDAL.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            var client = new Client(0, model.Firstname, model.Lastname, model.Email, model.Password, model.PhoneNumber);
            await _clientDAL.CreateAsync(client);

            Client? created = await _clientDAL.GetByEmailAndPasswordAsync(model.Email, model.Password);
            if (created != null)
                StoreClientInSession(created);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(SessionKeyId);
            HttpContext.Session.Remove(SessionKeyFirstname);
            HttpContext.Session.Remove(SessionKeyLastname);
            HttpContext.Session.Remove(SessionKeyUserType);
            return RedirectToAction("Index", "Home");
        }

        // --- Helpers ---

        private void StoreClientInSession(Client client)
        {
            HttpContext.Session.SetInt32(SessionKeyId, client.Id);
            HttpContext.Session.SetString(SessionKeyFirstname, client.Firstname);
            HttpContext.Session.SetString(SessionKeyLastname,  client.Lastname);
            HttpContext.Session.SetString(SessionKeyUserType,  client.UserType);
        }

        private IActionResult RedirectBasedOnRole(ISession session)
        {
            return GetUserType(session) switch
            {
                "cashier"     => RedirectToAction("Cashier", "Employee"),
                "orderpicker" => RedirectToAction("Index",   "Employee"),
                _             => RedirectToAction("Index",   "Home")
            };
        }

        // Static helpers — usable from views and other controllers via Context.Session
        public static bool IsLoggedIn(ISession session)
            => session.GetInt32(SessionKeyId).HasValue;

        public static string? GetUserType(ISession session)
            => session.GetString(SessionKeyUserType);

        public static bool IsEmployee(ISession session)
        {
            string? t = GetUserType(session);
            return t == "cashier" || t == "orderpicker";
        }

        public static string? GetClientFullname(ISession session)
        {
            string? firstname = session.GetString(SessionKeyFirstname);
            if (firstname == null) return null;
            return $"{firstname} {session.GetString(SessionKeyLastname)}";
        }
    }
}
