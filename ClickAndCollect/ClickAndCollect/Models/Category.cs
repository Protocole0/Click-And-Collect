using ClickAndCollect.Interfaces;

namespace ClickAndCollect.Models
{
    public class Category
    {
        private int _categoryId;
        private string _name;
        private string? _imageUrl;
        private string? _description;
        private List<Product> _products;

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

        public Category()
        {
            _name     = string.Empty;
            _products = new List<Product>();
        }

        public Category(int categoryId, string name, string? imageUrl, string? description)
        {
            _categoryId  = categoryId;
            _name        = name;
            _imageUrl    = imageUrl;
            _description = description;
            _products    = new List<Product>();
        }

        public Category(string name)
        {
            _name     = name;
            _products = new List<Product>();
        }

        // --- Static methods : the Category class delegates to the DAL ---

        public static async Task<List<Category>> GetAll(ICategoryDAL categoryDal)
        {
            return await categoryDal.GetAllAsync();
        }

        public static async Task<Category?> GetById(ICategoryDAL categoryDal, int categoryId)
        {
            return await categoryDal.GetByIdAsync(categoryId);
        }

        // --- Instance methods : the Category object manages its product list ---

        public void AddProduct(Product product)
        {
            foreach (var p in _products)
                if (p.ProductId == product.ProductId)
                    return;
            _products.Add(product);
        }

        public async Task LoadProductsAsync(IProductDAL productDal)
        {
            List<Product> products = await Product.GetByCategoryId(_categoryId, productDal);
            foreach (var product in products)
                AddProduct(product);
        }

        public List<Product> GetProducts()
        {
            return _products;
        }
    }
}
