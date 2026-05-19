using ClickAndCollect.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using ClickAndCollect.ViewModels;

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
        public static decimal DefaultServiceFee = 5.95m;

        // --- Liste des lignes (panier session ET commande BD) ---
        private List<OrderLine> _lines;
        public List<OrderLine> Lines
        {
            get => _lines;
            set { _lines = value; }
        }

        // --- Champs commande BD ---

        private int _id;
        public int Id { 
            get => _id; 
            set => _id = value;
        }

        private DateTime _orderDate;
        public DateTime OrderDate { 
            get => _orderDate; 
            set => _orderDate = value;
        }

        private decimal _serviceFee;
        public decimal ServiceFee { 
            get => _serviceFee; 
            set => _serviceFee = value; 
        }

        private int _cratesUsed;
        public int CratesUsed { 
            get => _cratesUsed; 
            set => _cratesUsed = value; 
        }

        private int _cratesReturned;
        public int CratesReturned { 
            get => _cratesReturned; 
            set => _cratesReturned = value; 
        }

        private OrderStatus _status;
        public OrderStatus Status { 
            get => _status; 
            set => _status = value; 
        }

        private Client? _client;
        public Client? Client { 
            get => _client; 
            set => _client = value; 
        }

        // --- Constructeurs ---

        public Order()
        {
            _lines = new List<OrderLine>();
        }

        public Order(int id, Client client)
        {
            _id    = id;
            _client = client;
            _lines = new List<OrderLine>();
        }

        public Order(int id, DateTime orderDate, int cratesUsed, int cratesReturned, OrderStatus status, Client client)
        {
            _id             = id;
            _orderDate      = orderDate;
            _cratesUsed     = cratesUsed;
            _cratesReturned = cratesReturned;
            _status         = status;
            _client         = client;
            _lines          = new List<OrderLine>();
        }




        // --- Méthodes panier (session) ---

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

        // --- Méthodes BD (statiques) ---

        public static async Task<List<OrderViewModel>> GetAllOrdersAsync(IOrderDAL orderDAL, OrderStatus status, int storeId)
        {
            return await orderDAL.GetAllOrdersAsync(status, storeId);
        }

        public static async Task<Order> GetOrderAsync(IOrderDAL orderDAL, int orderId)
        {
            return await orderDAL.GetOrderAsync(orderId);
        }

        public async Task<bool> UpdateCratesUsed(IOrderDAL orderDAL, int orderId, int cratesCount, int checkedProductsCount, OrderStatus status)
        {
            if(Lines.Count() == checkedProductsCount && cratesCount > 0)
            {
                return await orderDAL.UpdateCratesUsed(orderId, cratesCount, status);
            }
            return false;
        }
    }
}
