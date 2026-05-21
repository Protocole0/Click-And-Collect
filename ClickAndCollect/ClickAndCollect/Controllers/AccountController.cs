using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using ClickAndCollect.Services;
using ClickAndCollect.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserDAL _userDAL;
        private readonly IEmailService _emailService;

        public const string SessionKeyId = "user_id";
        public const string SessionKeyFirstname = "client_firstname";
        public const string SessionKeyLastname = "client_lastname";
        public const string SessionKeyUserType = "user_type";
        public const string SessionKeyEmail = "user_email";
        public const string SessionKeyStoreId = "store_id";

        public AccountController(IUserDAL userDAL, IEmailService emailService)
        {
            _userDAL = userDAL;
            _emailService = emailService;
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

            User? user = await Models.User.Login(model.Email, model.Password, _userDAL);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return View(model);
            }

            StoreUserInSession(user);
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

            if (await Client.EmailExists(model.Email, _userDAL))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            Client client;
            try
            {
                client = new Client(0, model.Firstname, model.Lastname, model.Email, model.Password, model.PhoneNumber);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            await client.CreateAccount(_userDAL);

            await _emailService.SendWelcomeEmailAsync(model.Email, model.Firstname, model.Lastname, model.Email);

            User? created = await Models.User.Login(model.Email, model.Password, _userDAL);
            if (created != null)
                StoreUserInSession(created);

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
            HttpContext.Session.Remove(SessionKeyEmail);
            HttpContext.Session.Remove(SessionKeyStoreId);
            return RedirectToAction("Index", "Home");
        }

        // --- Helpers ---

        private void StoreUserInSession(User user)
        {
            HttpContext.Session.SetInt32(SessionKeyId, user.Id);
            HttpContext.Session.SetString(SessionKeyUserType, user.UserType);
            HttpContext.Session.SetString(SessionKeyEmail, user.Email);

            if (user is Client client)
            {
                HttpContext.Session.SetString(SessionKeyFirstname, client.Firstname);
                HttpContext.Session.SetString(SessionKeyLastname, client.Lastname);
            }
            else if (user is Cashier cashier && cashier.Store != null)
            {
                HttpContext.Session.SetInt32(SessionKeyStoreId, cashier.Store.StoreId);
            }
            else if (user is OrderPicker picker && picker.Store != null)
            {
                HttpContext.Session.SetInt32(SessionKeyStoreId, picker.Store.StoreId);
            }
        }

        private IActionResult RedirectBasedOnRole(ISession session)
        {
            return GetUserType(session) switch
            {
                "client" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Dashboard", "Employee")
            };
        }

        // Static helpers — usable from views via Context.Session
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
