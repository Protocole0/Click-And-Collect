using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ClickAndCollect.Models
{
    public class Order
    {
        public const decimal ServiceFee = 5.95m;

        private List<OrderLine> _lines;

        public List<OrderLine> Lines
        {
            get => _lines;
            set { _lines = value; }
        }

        public int TotalItems()
        {
            int total = 0;
            foreach (OrderLine line in _lines)
                total += line.Quantity;
            return total;
        }

        public decimal TotalAmount()
        {
            decimal total = 0;
            foreach (OrderLine line in _lines)
                total += line.Quantity * line.Product.Price;
            return total;
        }

        public Order()
        {
            _lines = new List<OrderLine>();
        }

        // --- Méthode d'instance : l'objet Order communique avec la classe Product ---

        public void AddProduct(Product product, int quantity)
        {
            foreach (OrderLine line in _lines)
            {
                if (line.Product.ProductId == product.ProductId)
                {
                    line.Quantity += quantity;
                    return;
                }
            }
            _lines.Add(new OrderLine(product, quantity));
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            foreach (OrderLine line in _lines)
            {
                if (line.Product.ProductId == productId)
                {
                    line.Quantity = quantity;
                    return;
                }
            }
        }

        public void RemoveLine(int productId)
        {
            foreach (OrderLine line in _lines)
            {
                if (line.Product.ProductId == productId)
                {
                    _lines.Remove(line);
                    return;
                }
            }
        }

        // --- Méthodes statiques : gestion de la session ---

        public static Order GetFromSession(ISession session)
        {
            string? json = session.GetString("cart");
            if (json == null)
                return new Order();
            return JsonSerializer.Deserialize<Order>(json) ?? new Order();
        }

        public void SaveToSession(ISession session)
        {
            session.SetString("cart", JsonSerializer.Serialize(this));
        }
﻿using ClickAndCollect.Interfaces;

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
