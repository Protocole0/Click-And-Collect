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

        // Source unique de vérité pour la catégorie
        public Category? Category
        {
            get { return _category; }
            set { _category = value; }
        }

        // Propriétés calculées depuis l'objet Category
        public int CategoryId   => _category?.CategoryId ?? 0;

        public string? CategoryName => _category?.Name;

        // --- Constructeurs ---

        public Product() { _name = string.Empty; }

        // Constructeur utilisé par OrderDAL (nom + image + catégorie uniquement)
        public Product(string name, string? imageUrl, Category? category)
        {
            _name     = name;
            _imageUrl = imageUrl;
            _category = category;
        }

        // Constructeur principal utilisé par ProductDAL
        public Product(int productId, string name, string? description, decimal price,
                       string? imageUrl, string? nutritionalInfo, Category? category = null)
        {
            _productId       = productId;
            _name            = name;
            _description     = description;
            _price           = price;
            _imageUrl        = imageUrl;
            _nutritionalInfo = nutritionalInfo;
            _category        = category;
        }

        // --- Méthodes statiques : la classe délègue au DAL ---

        public static async Task<List<Product>> GetByCategoryId(int categoryId, IProductDAL productDal)
        {
            return await productDal.GetByCategoryIdAsync(categoryId);
        }

        public static async Task<Product?> GetById(int id, IProductDAL productDal)
        {
            return await productDal.GetByIdAsync(id);
        }

        // --- Méthode d'instance : l'objet Product communique avec la classe Category ---

        public Category GetCategory()
        {
            if (_category == null)
                throw new InvalidOperationException("La catégorie n'est pas chargée.");
            return _category;
        }
    }
}
