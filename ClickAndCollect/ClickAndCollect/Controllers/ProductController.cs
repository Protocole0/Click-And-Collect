using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ClickAndCollect.Controllers
{
    public class ProductController : Controller
    {
        private readonly ICategoryDal _categoryDal;
        private readonly IProductDal _productDal;

        private const string SessionKeyProducts = "products_browse";

        public ProductController(ICategoryDal categoryDal, IProductDal productDal)
        {
            _categoryDal = categoryDal;
            _productDal  = productDal;
        }

        // UC-3 : Select Category — 1 appel BD
        public IActionResult Index()
        {
            List<Category> categories = Category.GetAll(_categoryDal);
            return View(categories);
        }

        // UC-4 : Browse Product — 1 appel BD + stockage en session
        public IActionResult Browse(int id)
        {
            List<Product> products = Product.GetByCategoryId(id, _productDal);

            HttpContext.Session.SetString(SessionKeyProducts, JsonSerializer.Serialize(products));

            return View(products);
        }

        // UC-4 : Détail produit — 0 appel BD si produit déjà en session, sinon 1 appel BD (fallback)
        public IActionResult Details(int id)
        {
            Product? product = Product.GetFromSession(id, HttpContext.Session, SessionKeyProducts);

            if (product == null)
                product = Product.GetById(id, _productDal);

            if (product == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProductDetails", product);

            return View(product);
        }
    }
}
