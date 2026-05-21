namespace ClickAndCollect.Models
{
    public class OrderLine
    {
        private int _id;
        public int Id { 
            get => _id; 
            set
            {
                if (value < 0)
                    throw new ArgumentException("L'id d'une ligne de commande ne peut être négatif");
                _id = value;
            }
        }

        private Product _product;
        public Product Product { 
            get => _product; 
            set => _product = value; 
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => _quantity = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), "La quantité doit être supérieure à 0.");
        }

        public decimal GetSubTotal()
        {
            return _quantity * _product.Price;
        }

        public OrderLine() { _product = new Product(); }

        // Constructeur panier
        public OrderLine(Product product, int quantity)
        {
            _product = product;
            Quantity = quantity;
        }

        // Constructeur commande BD
        public OrderLine(int id, int quantity, Product product)
        {
            _id      = id;
            Quantity = quantity;
            _product = product;
        }
    }
}
