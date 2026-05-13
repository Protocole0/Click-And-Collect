using ClickAndCollect.Interfaces;

namespace ClickAndCollect.Models
{
    public class Category
    {
        private int _categoryId;
        private string _name;
        private string? _imageUrl;
        private string? _description;
        private List<Product>? _products;

        public int CategoryId
        {
            get { return _categoryId; }
            set { _categoryId = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string? ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; }
        }

        public string? Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public Category(int categoryId, string name, string? imageUrl, string? description)
        {
            _categoryId  = categoryId;
            _name        = name;
            _imageUrl    = imageUrl;
            _description = description;
        }

        // --- Méthode statique : la classe Category délègue au DAL ---

        public static List<Category> GetAll(ICategoryDal categoryDal)
        {
            return categoryDal.GetAll();
        }

        // --- Méthode d'instance : l'objet Category communique avec la classe Product ---

        public void LoadProducts(IProductDal productDal)
        {
            _products = Product.GetByCategoryId(this._categoryId, productDal);
        }

        public List<Product> GetProducts()
        {
            if (_products == null)
                throw new InvalidOperationException("Les produits ne sont pas chargés. Appelez LoadProducts() d'abord.");
            return _products;
        }
    }
}
