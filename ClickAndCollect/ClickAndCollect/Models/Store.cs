namespace ClickAndCollect.Models
{
	public class Store
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

		private string _streetName;

		public string StreetName
		{
			get { return _streetName; }
			set { _streetName = value; }
		}

		private string _streetNumber;

		public string StreetNumber
		{
			get { return _streetNumber; }
			set { _streetNumber = value; }
		}

		private string _city;

		public string City
		{
			get { return _city; }
			set { _city = value; }
		}

		private string _postalCode;

		public string PostalCode
		{
			get { return _postalCode; }
			set { _postalCode = value; }
		}

		private List<User> _employees;

		public List<User> Employees
		{
			get { return _employees; }
			set { _employees = value; }
		}

		public Store()
		{
			_employees = new List<User>();
        }

		public Store(int id, string name, string streetName, string streetNumber, string city, string postalCode)
		{
			_id = id;
			_name = name;
			_streetName = streetName;
			_streetNumber = streetNumber;
			_city = city;
			_postalCode = postalCode;
			_employees = new List<User>();
		}
    }
}
