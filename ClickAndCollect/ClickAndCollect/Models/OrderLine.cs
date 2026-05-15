namespace ClickAndCollect.Models
{
	public class OrderLine
	{
		private int _id;

		public int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _quantity;

		public int Quantity
		{
			get { return _quantity; }
			set { _quantity = value; }
		}

		private bool isChecked;

		private Product _product;

		public Product Product
		{
			get { return _product; }
			set { _product = value; }
		}

        public OrderLine(int id, int quantity, Product product)
        {
			Id = id;
			Quantity = quantity;
			Product = product;
        }

    }
}

