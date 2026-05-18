using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using ClickAndCollect.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ClickAndCollect.Controllers
{
    public class OrderController : Controller
    {
        private readonly IStoreDAL     _storeDAL;
        private readonly ITimeSlotDAL  _timeSlotDAL;
        private readonly IOrderDAL     _orderDAL;
        private readonly IEmailService _emailService;

        public OrderController(IStoreDAL storeDAL, ITimeSlotDAL timeSlotDAL, IOrderDAL orderDAL, IEmailService emailService)
        {
            _storeDAL     = storeDAL;
            _timeSlotDAL  = timeSlotDAL;
            _orderDAL     = orderDAL;
            _emailService = emailService;
        }

        // GET: /Order/Checkout — page unique magasin + date + créneau + récap
        public async Task<IActionResult> Checkout()
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Account");

            Order cart = Order.GetFromSession(HttpContext.Session);
            if (cart.TotalItems() == 0)
                return RedirectToAction("Index", "Cart");

            ViewBag.Stores = await Store.GetAll(_storeDAL);
            return View(cart);
        }

        // AJAX GET: /Order/GetDates?storeId=X
        // Retourne les dates disponibles (à partir de demain, au moins 1 créneau libre)
        public async Task<IActionResult> GetDates(int storeId)
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            List<DateTime> dates = await TimeSlot.GetAvailableDates(storeId, _timeSlotDAL);

            var result = dates.Select(d => new
            {
                value = d.ToString("yyyy-MM-dd"),
                label = d.ToString("dddd d MMMM yyyy", new CultureInfo("fr-FR"))
            });

            return Json(result);
        }

        // AJAX GET: /Order/GetTimeSlots?storeId=X&date=yyyy-MM-dd
        // Retourne les créneaux disponibles (< 10 réservations) pour ce magasin et cette date
        public async Task<IActionResult> GetTimeSlots(int storeId, string date)
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            List<TimeSlot> slots = await TimeSlot.GetAvailableByDate(parsedDate, storeId, _timeSlotDAL);

            var result = slots.Select(s => new
            {
                timeSlotId = s.TimeSlotId,
                label      = $"{s.StartTime:hh\\:mm} → {s.EndTime:hh\\:mm}",
                placesLeft = s.PlacesLeft
            });

            return Json(result);
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int storeId, int timeSlotId)
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Account");

            Order cart = Order.GetFromSession(HttpContext.Session);
            if (cart.TotalItems() == 0)
                return RedirectToAction("Index", "Cart");

            int    clientId  = HttpContext.Session.GetInt32(AccountController.SessionKeyId)!.Value;
            string firstname = HttpContext.Session.GetString(AccountController.SessionKeyFirstname)!;
            string lastname  = HttpContext.Session.GetString(AccountController.SessionKeyLastname)!;
            string email     = HttpContext.Session.GetString(AccountController.SessionKeyEmail) ?? string.Empty;

            // Construire le client via son constructeur
            var client = new Client(clientId, firstname, lastname, string.Empty);

            // Construire chaque ligne via le constructeur OrderLine(Product, quantity)
            var lines = cart.Lines
                .Select(l => new OrderLine(l.Product, l.Quantity))
                .ToList();

            // Construire la commande complète via le constructeur dédié
            var order = new Order(
                id:             0,
                orderDate:      DateTime.Now,
                cratesUsed:     0,
                cratesReturned: 0,
                status:         OrderStatus.PENDING_PREPARATION,
                client:         client,
                lines:          lines,
                storeId:        storeId,
                timeSlotId:     timeSlotId);

            // Persister via la méthode d'instance
            await order.PlaceOrder(_orderDAL);

            // Envoyer l'email de confirmation (on récupère le magasin et le créneau pour le détail)
            Store?    confirmedStore = await Store.GetById(storeId, _storeDAL);
            TimeSlot? confirmedSlot  = await TimeSlot.GetById(timeSlotId, _timeSlotDAL);
            if (confirmedStore != null && confirmedSlot != null && !string.IsNullOrEmpty(email))
                await _emailService.SendOrderConfirmationEmailAsync(email, firstname, lastname, order, confirmedStore, confirmedSlot);

            // Vider le panier session
            new Order().SaveToSession(HttpContext.Session);

            return RedirectToAction("Success");
        }

        // GET: /Order/Success
        public IActionResult Success()
        {
            return View();
        }
    }
}
