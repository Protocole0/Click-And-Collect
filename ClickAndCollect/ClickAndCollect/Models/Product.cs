using ClickAndCollect.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ClickAndCollect.Models
{
    public class Product
    {
        private int _productId;
        private string _name;
        private string? _description;
        private decimal _price;
        private string? _imageUrl;
        private string? _nutritionalInfo;
        private int _categoryId;
        private Category? _category;

        public int ProductId
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string? Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public string? ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; }
        }

        public string? NutritionalInfo
        {
            get { return _nutritionalInfo; }
            set { _nutritionalInfo = value; }
        }

        public int CategoryId
        {
            get { return _categoryId; }
            set { _categoryId = value; }
        }

        // Délègue à l'objet Category — pas de duplication de données
        public string? CategoryName => _category?.Name;

        public Product() { _name = string.Empty; }

        public Product(int productId, string name, string? description, decimal price,
                       string? imageUrl, string? nutritionalInfo, int categoryId, Category? category = null)
        {
            _productId       = productId;
            _name            = name;
            _description     = description;
            _price           = price;
            _imageUrl        = imageUrl;
            _nutritionalInfo = nutritionalInfo;
            _categoryId      = categoryId;
            _category        = category;
        }

        // --- Méthodes statiques : la classe délègue au DAL ---

        public static async Task<List<Product>> GetByCategoryId(int categoryId, IProductDal productDal)
        {
            return await productDal.GetByCategoryIdAsync(categoryId);
        }

        public static async Task<Product?>GetById(int id, IProductDal productDal)
        {
            return await productDal.GetByIdAsync(id);
        }

        // Parcourt la liste en session et retourne le produit correspondant à l'id
        public static Product? GetFromSession(int id, ISession session, string sessionKey)
        {
            string? json = session.GetString(sessionKey);
            if (json == null)
                return null;

            List<Product>? products = JsonSerializer.Deserialize<List<Product>>(json);
            return products?.FirstOrDefault(p => p._productId == id);
        }

        // --- Méthode d'instance : l'objet Product communique avec la classe Category ---

        public async Task LoadCategoryAsync(ICategoryDal categoryDal)
        {
            List<Category> all = await Category.GetAll(categoryDal);
            _category = all.FirstOrDefault(c => c.CategoryId == _categoryId);
        }

        public Category GetCategory()
        {
            if (_category == null)
                throw new InvalidOperationException("La catégorie n'est pas chargée. Appelez LoadCategory() d'abord.");
            return _category;
        }
    }
}
