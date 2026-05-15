using ClickAndCollect.Interfaces;

namespace ClickAndCollect.Models
{
	public enum OrderStatus
	{
		DRAFT,
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

		private Client _client;
		public Client Client
		{
			get { return _client; }
			set { _client = value; }
		}

        private List<OrderLine> _orderlines;
        public List<OrderLine> OrderLines
        {
            get { return _orderlines; }
            set { _orderlines = value; }
        }

        // Constructor to display order lines in the order picker order preview
        public Order(int id)
		{
			Id = id;
			OrderLines = new List<OrderLine>();
        }
        
		public Order(int id, Client client)
		{
			Id = id;
			Client = client;
			CratesUsed = 0;
			CratesReturned = 0;
			Status = OrderStatus.PENDING_PREPARATION;
		}
		
		public Order(int id, DateTime orderDate, int crates_used, int crates_returned, OrderStatus status, Client client)
		{
			Id = id;
			OrderDate = orderDate;
			CratesUsed = crates_used;
			CratesReturned = crates_returned;
			Status = status;
			Client = client;
			OrderLines = new List<OrderLine>();
		}

        public static async Task<List<Order>> GetAllOrdersAsync(IOrderDAL orderDAL, OrderStatus status)
        {
            return await orderDAL.GetAllOrdersAsync(status);
        }
        
		public static async Task<Order> GetOrderAsync(IOrderDAL orderDAL, int orderId)
        {
            return await orderDAL.GetOrderAsync(orderId);
        }




    }
}
