namespace ClickAndCollect.ViewModels
{
    public class OrderLineDisplayViewModel
    {
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }

        public OrderLineDisplayViewModel(int quantity, decimal productPrice)
        {
            Quantity = quantity;
            ProductPrice = productPrice;
        }
    }
}
