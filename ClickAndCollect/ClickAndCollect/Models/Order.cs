using ClickAndCollect.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

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

        private int _storeId;
        public int StoreId { get => _storeId; set => _storeId = value; }

        private int _timeSlotId;
        public int TimeSlotId { get => _timeSlotId; set => _timeSlotId = value; }

        // --- Constructeurs ---

        public Order()
        {
            _lines = new List<OrderLine>();
        }

        public Order(int id)
        {
            _id    = id;
            _lines = new List<OrderLine>();
        }

        public Order(int id, Client client)
        {
            _id             = id;
            _client         = client;
            _cratesUsed     = 0;
            _cratesReturned = 0;
            _status         = OrderStatus.PENDING_PREPARATION;
            _lines          = new List<OrderLine>();
        }

        // Constructeur complet pour valider une commande client
        public Order(int id, DateTime orderDate, int cratesUsed, int cratesReturned,
                     OrderStatus status, Client client, List<OrderLine> lines,
                     int storeId, int timeSlotId)
        {
            _id             = id;
            _orderDate      = orderDate;
            _cratesUsed     = cratesUsed;
            _cratesReturned = cratesReturned;
            _status         = status;
            _client         = client;
            _lines          = lines;
            _storeId        = storeId;
            _timeSlotId     = timeSlotId;
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

        // --- Méthodes BD ---

        public static async Task<List<Order>> GetAllOrdersAsync(IOrderDAL orderDAL, OrderStatus status)
        {
            return await orderDAL.GetAllOrdersAsync(status);
        }

        public static async Task<Order> GetOrderAsync(IOrderDAL orderDAL, int orderId)
        {
            return await orderDAL.GetOrderAsync(orderId);
        }

        // Méthode d'instance : l'objet Order se persiste en BD via le DAL
        public async Task PlaceOrder(IOrderDAL orderDAL)
        {
            await orderDAL.CreateAsync(this);
        }
    }
}
