using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ClickAndCollect.Controllers
{
    public class ProductController : Controller
    {
        private readonly ICategoryDAL _categoryDAL;
        private readonly IProductDAL _productDAL;

        private const string SessionKeyProducts = "products_browse";

        public ProductController(ICategoryDAL categoryDAL, IProductDAL productDAL)
        {
            _categoryDAL = categoryDAL;
            _productDAL  = productDAL;
        }

        // UC-3 : Select Category — 1 appel BD
        public async Task<IActionResult> Index()
        {
            List<Category> categories = await Category.GetAll(_categoryDAL);
            return View(categories);
        }

        // UC-4 : Browse Product — 1 appel BD + stockage en session
        public async Task<IActionResult> Browse(int id)
        {
            List<Product> products = await Product.GetByCategoryId(id, _productDAL);

            HttpContext.Session.SetString(SessionKeyProducts, JsonSerializer.Serialize(products));

            return View(products);
        }

        // UC-4 : Détail produit — 0 appel BD si produit déjà en session, sinon 1 appel BD (fallback)
        public async Task<IActionResult> Details(int id)
        {
            Product? product = Product.GetFromSession(id, HttpContext.Session, SessionKeyProducts);

            if (product == null)
                product = await Product.GetById(id, _productDAL);

            if (product == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProductDetails", product);

            return View(product);
        }
    }
}
