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
	}
}

