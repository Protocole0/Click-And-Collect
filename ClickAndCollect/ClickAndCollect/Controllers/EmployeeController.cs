using Microsoft.AspNetCore.Mvc;
using ClickAndCollect.Models;
using ClickAndCollect.ViewModels;
using ClickAndCollect.Interfaces;

namespace ClickAndCollect.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IOrderDAL _orderDAL;

        public EmployeeController(IOrderDAL orderDAL)
        {
            _orderDAL = orderDAL;
        }

        public async Task<IActionResult> Index()
        {
            int storeId = HttpContext.Session.GetInt32("store_id") ?? 2;
            List<OrderViewModel> orders = await Order.GetAllOrdersAsync(_orderDAL, OrderStatus.PENDING_PREPARATION, storeId);
            return View(orders);
        }

        public async Task<IActionResult> Cashier()
        {
            int storeId = HttpContext.Session.GetInt32("store_id") ?? 2;
            List<OrderViewModel> orders = await Order.GetAllOrdersAsync(_orderDAL, OrderStatus.READY_FOR_PICKUP, storeId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderContent(int orderId)
        {
            Order order = await Order.GetOrderAsync(_orderDAL, orderId);
            return PartialView("_OrderContent", order);
        }

        [HttpPost]
        public async Task<IActionResult> ValidatePreparation(int orderId, int cratesUsed, List<int> checkedProducts)
        {
            Order orderPrepared = await Order.GetOrderAsync(_orderDAL, orderId);

            int  checkedProductsCount = checkedProducts.Count();
            bool success = await orderPrepared.UpdateCratesUsed(_orderDAL, orderId, cratesUsed, checkedProductsCount, OrderStatus.READY_FOR_PICKUP);

            TempData["SuccessMessage"] = $"La commande n°{orderPrepared.Id} de {orderPrepared.Client!.Firstname} {orderPrepared.Client.Lastname} a bien été validée et est prête au retrait !";

            return RedirectToAction(nameof(Index));
        }
    }
}
