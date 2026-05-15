namespace ClickAndCollect.Models
{
    public class OrderLine
    {
        private int _id;
        public int Id { 
            get => _id; 
            set => _id = value; 
        }

        private Product _product;
        public Product Product { 
            get => _product; 
            set => _product = value; 
        }

        private int _quantity;
        public int Quantity { 
            get => _quantity; 
            set => _quantity = value; 
        }

        public decimal SubTotal => Quantity * Product.Price;

        public OrderLine() { _product = new Product(); }

        // Constructeur panier
        public OrderLine(Product product, int quantity)
        {
            _product  = product;
            _quantity = quantity;
        }

        // Constructeur commande BD
        public OrderLine(int id, int quantity, Product product)
        {
            _id       = id;
            _quantity = quantity;
            _product  = product;
        }
    }
}
