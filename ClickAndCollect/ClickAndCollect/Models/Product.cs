using ClickAndCollect.Interfaces;

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
            set { _price = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), "Le prix doit être supérieur à 0."); }
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

        // Single source of truth for the category
        public Category? Category
        {
            get { return _category; }
            set { _category = value; }
        }

        // Computed properties from the Category object
        public int CategoryId   => _category?.CategoryId ?? 0;

        public string? CategoryName => _category?.Name;

        // --- Constructors ---

        public Product() { _name = string.Empty; }
        
        // Constructor with the price only, for the cashier
        public Product(decimal price) { Price = price; }

        // Constructor used by OrderDAL (name + image + category only)
        public Product(string name, string? imageUrl, Category? category)
        {
            _name     = name;
            _imageUrl = imageUrl;
            _category = category;
        }

        // Constructor used by CartController
        public Product(int id, string name, decimal price, string? imageUrl)
        {
            _productId = id;
            _name      = name;
            Price      = price;
            _imageUrl  = imageUrl;
        }

        // Main constructor used by ProductDAL
        public Product(int productId, string name, string? description, decimal price,
                       string? imageUrl, string? nutritionalInfo, Category? category = null)
        {
            _productId       = productId;
            _name            = name;
            _description     = description;
            Price            = price;
            _imageUrl        = imageUrl;
            _nutritionalInfo = nutritionalInfo;
            _category        = category;
        }

        // --- Static methods : the class delegates to the DAL ---

        public static async Task<List<Product>> GetByCategoryId(int categoryId, IProductDAL productDal)
        {
            return await productDal.GetByCategoryIdAsync(categoryId);
        }

        public static async Task<Product?> GetById(int id, IProductDAL productDal)
        {
            return await productDal.GetByIdAsync(id);
        }

        public Category GetCategory()
        {
            if (_category == null)
                throw new InvalidOperationException("La catégorie n'est pas chargée.");
            return _category;
        }
    }
}
