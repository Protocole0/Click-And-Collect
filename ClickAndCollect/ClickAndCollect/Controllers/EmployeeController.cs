using Microsoft.AspNetCore.Mvc;
using ClickAndCollect.Models;
using ClickAndCollect.Interfaces;
using ClickAndCollect.DAL;
using System.Threading.Tasks;

namespace ClickAndCollect.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IOrderDAL _orderDAL;

        public EmployeeController(IOrderDAL orderDAL)
        {
            _orderDAL = orderDAL;
        }
        public async Task<IActionResult> Index() {
            List<Order> orders = await Order.GetAllOrdersAsync(_orderDAL, OrderStatus.PENDING_PREPARATION);

            return View(orders);
        }
        
        public async Task<IActionResult> OrderPreview(int orderId)
        {
            Order order = await Order.GetOrderAsync(_orderDAL, orderId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_OrderPreview", order);

            return View(order);
        }
    }
}
