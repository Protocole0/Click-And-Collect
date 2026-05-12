namespace ClickAndCollect.Models
{
	public enum OrderStatus
	{
		PENDING_PREPARATION,
		READY_FOR_PICKUP,
		COLLECTED
    }
    public class Order
    {
		private int _id;

		public int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private DateTime _orderDate;

		public DateTime OrderDate
		{
			get { return _orderDate; }
			set { _orderDate = value; }
		}

		private decimal _serviceFee;

		public decimal ServiceFee
		{
			get { return _serviceFee; }
			set { _serviceFee = value; }
		}

		private int _cratesUsed;

		public int CratesUsed
		{
			get { return _cratesUsed; }
			set { _cratesUsed = value; }
		}
		
		private int _cratesReturned;

		public int CratesReturned
		{
			get { return _cratesReturned; }
			set { _cratesReturned = value; }
		}

		private OrderStatus _status;

		public OrderStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}




	}
}
