using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using ClickAndCollect.Services;
using ClickAndCollect.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IOrderDAL _orderDAL;
        private readonly IEmailService _emailService;

        public EmployeeController(IOrderDAL orderDAL, IEmailService emailService)
        {
            _orderDAL = orderDAL;
            _emailService = emailService;
        }

        // UC-10, UC-13
        // With this action, you can open the view
        // of both a cashier and a preparer
        // just by checking his user_type
        public async Task<IActionResult> Dashboard()
        {
            User employee = null;
            string? user_type = HttpContext.Session.GetString("user_type");
            int? id = HttpContext.Session.GetInt32("user_id");
            string? user_email = HttpContext.Session.GetString("user_email");
            // store_id take the store_id of the employee logged in
            // so that the employee can only see the orders of his store
            int? storeId = HttpContext.Session.GetInt32("store_id");
            List<OrderDisplayViewModel> orders = null;

            if (user_type == "orderpicker")
            {
                employee = new OrderPicker(user_type, id, user_email, new Store(storeId));
                orders = await ((OrderPicker)employee).GetAllOrdersAsync(_orderDAL);
                return View("OrderPicker", orders);
            }
            else if (user_type == "cashier")
            {
                employee = new Cashier(user_type, id, user_email, new Store(storeId));
                orders = await ((Cashier)employee).GetAllOrdersAsync(_orderDAL);
                return View("Cashier", orders);
            }
            else
                return RedirectToAction("Index", "Home");
        }

        // UC-11, UC-12, UC-14, UC-15
        public async Task<IActionResult> OrderData(int orderId)
        {
            Order order = null;
            if (HttpContext.Session.GetString("user_type") == "orderpicker")
            {
                order = await Order.GetOrderForChecklistAsync(_orderDAL, orderId);
                return PartialView("_OrderPickerData", order);
            }
            order = await Order.GetOrderForBillAsync(_orderDAL, orderId);
            return PartialView("_CashierData", order);
        }

        [HttpPost]
        public async Task<IActionResult> ValidatePreparation(int orderId, int cratesUsed, List<int> checkedProducts)
        {
            Order orderPrepared = await Order.GetOrderForChecklistAsync(_orderDAL, orderId);

            int checkedProductsCount = checkedProducts.Count();
            try
            {
                // Make a try-catch because de the CratesUsed property could not be less than 1
                bool success = await orderPrepared.UpdateCratesUsed(_orderDAL, cratesUsed, checkedProductsCount);

                TempData["SuccessMessage"] = $"La commande n°{orderPrepared.Id} de {orderPrepared.Client!.Firstname} {orderPrepared.Client.Lastname} a bien été validée et est prête au retrait !";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }
       
        [HttpPost]
        public async Task<IActionResult> ValidatePickUp(int orderId, int cratesReturned)
        {
            Order orderPickedUp = await Order.GetOrderForBillAsync(_orderDAL, orderId);

            try
            {
                bool success = await orderPickedUp.UpdateCratesReturned(_orderDAL, cratesReturned);

                TempData["SuccessMessage"] = $"La commande n°{orderPickedUp.Id} de {orderPickedUp.Client!.Firstname} {orderPickedUp.Client.Lastname} a bien été retirée !";

                await _emailService.SendOrderFinalBillAsync(orderPickedUp);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
