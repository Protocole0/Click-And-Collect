using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductDal _productDal;
        private const string SessionKeyProducts = "products_browse";

        public CartController(IProductDal productDal)
        {
            _productDal = productDal;
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
            if (product == null) product = await Product.GetById(productId, _productDal);
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
                success    = true,
                cartCount  = cart.TotalItems(),
                subTotal   = cart.TotalAmount().ToString("0.00"),
                total      = (cart.TotalAmount() + Order.ServiceFee).ToString("0.00")
            });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            Order cart = Order.GetFromSession(HttpContext.Session);
            cart.RemoveLine(productId);
            cart.SaveToSession(HttpContext.Session);

            decimal fee = cart.Lines.Any() ? Order.ServiceFee : 0m;
            return Json(new
            {
                success   = true,
                cartCount = cart.TotalItems(),
                isEmpty   = !cart.Lines.Any(),
                subTotal  = cart.TotalAmount().ToString("0.00"),
                total     = (cart.TotalAmount() + fee).ToString("0.00")
            });
        }
    }
}
