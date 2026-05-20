using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using ClickAndCollect.ViewModels;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IOrderDAL _orderDAL;

        public EmployeeController(IOrderDAL orderDAL)
        {
            _orderDAL = orderDAL;
        }

        // With this action, you can open the view
        // of both a cashier and a preparer
        // just by checking his user_type
        public async Task<IActionResult> Dashboard()
        {
            // store_id take the store_id of the employee logged in
            // so that the employee can only see the orders of his store
            int? storeId = HttpContext.Session.GetInt32("store_id");
            List<OrderDisplayViewModel> orders = null;

            if (HttpContext.Session.GetString("user_type") == "orderpicker")
            {
                orders = await Order.GetAllOrdersAsync(_orderDAL, OrderStatus.PENDING_PREPARATION, storeId, (DateTime.Today).AddDays(1));
                return View("OrderPicker", orders);
            }
            // else if it's a cashier :
            orders = await Order.GetAllOrdersAsync(_orderDAL, OrderStatus.READY_FOR_PICKUP, storeId, DateTime.Today);
            return View("Cashier", orders);
        }

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
            bool success = await orderPrepared.UpdateCratesUsed(_orderDAL, orderId, cratesUsed, checkedProductsCount);

            TempData["SuccessMessage"] = $"La commande n°{orderPrepared.Id} de {orderPrepared.Client!.Firstname} {orderPrepared.Client.Lastname} a bien été validée et est prête au retrait !";

            return RedirectToAction(nameof(Dashboard));
        }
       
        [HttpPost]
        public async Task<IActionResult> ValidatePickUp(int orderId, int cratesReturned)
        {
            Order orderPickedUp = await Order.GetOrderForBillAsync(_orderDAL, orderId);

            bool success = await orderPickedUp.UpdateCratesReturned(_orderDAL, orderId, cratesReturned);

            TempData["SuccessMessage"] = $"La commande n°{orderPickedUp.Id} de {orderPickedUp.Client!.Firstname} {orderPickedUp.Client.Lastname} a bien été retirée !";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
