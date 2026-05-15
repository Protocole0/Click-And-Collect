namespace ClickAndCollect.Models
{
    public class OrderLine
    {
        private Product product = null;
        private int quantity;

        public Product Product { 
            get => product; 
            set => product = value; 
        } 

        public int Quantity { 
            get => quantity; 
            set=> quantity = value; 
        }

        public decimal SubTotal => Quantity * Product.Price;

        public OrderLine() { }

        public OrderLine(Product product, int quantity)
        {
            Product  = product;
            Quantity = quantity;
        }
    }
}
