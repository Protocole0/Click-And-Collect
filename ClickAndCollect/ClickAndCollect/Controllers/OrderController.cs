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

        // GET: /Order/Checkout — single page for store + date + time slot + order summary
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
        // Returns available dates (from tomorrow, at least one free slot)
        public async Task<IActionResult> GetDates(int storeId)
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            List<TimeSlot> available = await TimeSlot.GetAvailableAsync(storeId, _timeSlotDAL);

            var result = available
                .Select(s => s.DateSlot.Date)
                .Distinct()
                .Select(d => new
                {
                    value = d.ToString("yyyy-MM-dd"),
                    label = d.ToString("dddd d MMMM yyyy", new CultureInfo("fr-FR"))
                });

            return Json(result);
        }

        // AJAX GET: /Order/GetTimeSlots?storeId=X&date=yyyy-MM-dd
        // Returns available slots (less than 10 reservations) for the given store and date
        public async Task<IActionResult> GetTimeSlots(int storeId, string date)
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return Unauthorized();

            DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            List<TimeSlot> available = await TimeSlot.GetAvailableAsync(storeId, _timeSlotDAL);
            List<TimeSlot> slots = available.Where(s => s.DateSlot.Date == parsedDate.Date).ToList();

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

            // Build the client from session data
            var client = new Client(clientId, firstname, lastname, string.Empty);

            // Fetch the store and the slot to pass them to the order
            Store?    store = await Store.GetById(storeId, _storeDAL);
            TimeSlot? slot  = await TimeSlot.GetById(timeSlotId, _timeSlotDAL);

            if (store == null || slot == null)
                return RedirectToAction("Checkout");

            // Build each order line using the OrderLine(Product, quantity) constructor
            var lines = cart.Lines
                .Select(l => new OrderLine(l.Product, l.Quantity))
                .ToList();

            // Build the full order using the dedicated constructor
            var order = new Order(
                id:             0,
                orderDate:      DateTime.Now,
                cratesUsed:     0,
                cratesReturned: 0,
                status:         OrderStatus.PENDING_PREPARATION,
                client:         client,
                lines:          lines,
                store:          store,
                slot:           slot);

            // Save the order using the instance method
            await order.PlaceOrder(_orderDAL);

            // Send the confirmation email
            if (!string.IsNullOrEmpty(email))
                await _emailService.SendOrderConfirmationEmailAsync(email, firstname, lastname, order, order.Store!, order.Slot!);

            // Clear the session cart
            new Order().SaveToSession(HttpContext.Session);

            return RedirectToAction("Success");
        }

        // GET: /Order/Success
        public IActionResult Success()
        {
            return View();
        }

        // GET: /Order/History
        public async Task<IActionResult> History()
        {
            if (!AccountController.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Account");

            if (AccountController.IsEmployee(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            int    clientId  = HttpContext.Session.GetInt32(AccountController.SessionKeyId)!.Value;

            var client = new Client(clientId);
            List<Order> orders = await client.GetOrdersAsync(_orderDAL);

            return View(orders);
        }
    }
}
