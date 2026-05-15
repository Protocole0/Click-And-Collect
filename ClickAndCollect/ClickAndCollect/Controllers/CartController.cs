using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductDAL _productDAL;
        private const string SessionKeyProducts = "products_browse";

        public CartController(IProductDAL productDAL)
        {
            _productDAL = productDAL;
        }

        public IActionResult Index()
        {
            Order cart = Order.GetFromSession(HttpContext.Session);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            Product? product = Product.GetFromSession(productId, HttpContext.Session, SessionKeyProducts);
            if (product == null) product = await Product.GetById(productId, _productDAL);
            if (product == null) return NotFound();

            Order cart = Order.GetFromSession(HttpContext.Session);
            cart.AddProduct(product, quantity);
            cart.SaveToSession(HttpContext.Session);

            return Json(new { success = true, cartCount = cart.TotalItems() });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1) return BadRequest();

            Order cart = Order.GetFromSession(HttpContext.Session);
            cart.UpdateQuantity(productId, quantity);
            cart.SaveToSession(HttpContext.Session);

            return Json(new
            {
                success   = true,
                cartCount = cart.TotalItems(),
                subTotal  = cart.TotalAmount().ToString("0.00"),
                total     = (cart.TotalAmount() + Order.DefaultServiceFee).ToString("0.00")
            });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            Order cart = Order.GetFromSession(HttpContext.Session);
            cart.RemoveLine(productId);
            cart.SaveToSession(HttpContext.Session);

            decimal serviceFee = cart.Lines.Any() ? Order.DefaultServiceFee : 0m;
            return Json(new
            {
                success   = true,
                cartCount = cart.TotalItems(),
                isEmpty   = !cart.Lines.Any(),
                subTotal  = cart.TotalAmount().ToString("0.00"),
                total     = (cart.TotalAmount() + serviceFee).ToString("0.00")
            });
        }
    }
}
