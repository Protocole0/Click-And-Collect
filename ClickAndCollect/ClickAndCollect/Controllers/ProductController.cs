using ClickAndCollect.Interfaces;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClickAndCollect.Controllers
{
    public class ProductController : Controller
    {
        private readonly ICategoryDAL _categoryDAL;
        private readonly IProductDAL  _productDAL;

        public ProductController(ICategoryDAL categoryDAL, IProductDAL productDAL)
        {
            _categoryDAL = categoryDAL;
            _productDAL  = productDAL;
        }

        // UC-3 : Select Category
        public async Task<IActionResult> Index()
        {
            List<Category> categories = await Category.GetAll(_categoryDAL);
            return View(categories);
        }

        // UC-4 : Browse Product — charge la catégorie puis ses produits via LoadProductsAsync
        public async Task<IActionResult> Browse(int id)
        {
            Category? category = await Category.GetById(id, _categoryDAL);
            if (category == null)
                return NotFound();

            await category.LoadProductsAsync(_productDAL);

            return View(category);
        }

        // UC-4 : Détail produit — toujours récupéré depuis la BD
        public async Task<IActionResult> Details(int id)
        {
            Product? product = await Product.GetById(id, _productDAL);

            if (product == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProductDetails", product);

            return View(product);
        }
    }
}
