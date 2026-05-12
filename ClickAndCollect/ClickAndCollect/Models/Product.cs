namespace ClickAndCollect.Models
{
	public class Product
	{
		private int _id;

		public int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private string _description;

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		private decimal _price;

		public decimal Price
		{
			get { return _price; }
			set { _price = value; }
		}

		private string _nutritionalInfos;

		public string NutritionalInfos
		{
			get { return _nutritionalInfos; }
			set { _nutritionalInfos = value; }
		}

	}
}