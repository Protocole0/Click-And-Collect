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

        private Store? _store;
        public Store? Store { get => _store; set => _store = value; }

        private TimeSlot? _slot;
        public TimeSlot? Slot { get => _slot; set => _slot = value; }

        // --- Constructeurs ---

        public Order()
        {
            Lines = new List<OrderLine>();
        }

        // Constructors for the GetOrderForChecklist method
        // in the OrderDAL for the order picker data
        public Order(int orderId, Client client)
        {
            Id = orderId;
            Client = client;
            _lines = new List<OrderLine>();
        }
        
        // Constructors for the GetOrderForBill method
        // in the OrderDAL for the cashier data
        public Order(int orderId, int cratesUsed, Client client)
        {
            Id = orderId;
            CratesUsed = cratesUsed;
            Client = client;
            _lines = new List<OrderLine>();
        }

        // Constructeur complet pour valider une commande client
        public Order(int id, DateTime orderDate, int cratesUsed, int cratesReturned,
                     OrderStatus status, Client client, List<OrderLine> lines,
                     Store store, TimeSlot slot)
        {
            _id             = id;
            _orderDate      = orderDate;
            _cratesUsed     = cratesUsed;
            _cratesReturned = cratesReturned;
            _status         = status;
            _client         = client;
            _lines          = lines;
            _store          = store;
            _slot           = slot;
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

        public decimal GetHTTotalPrice()
        {
            // Using of lambda expression in the LINQ Sum method
            // to calculate total price of an order without
            // including the crate fee and the service fee
            return Lines.Sum(l => l.Quantity * l.Product.Price);
        }

        public decimal GetTTCTotalPrice()
        {
            // Calculate total price of an order including
            // the price of products (GetHTTotalPrice), the crate fee and the service fee
            decimal serviceFee = DefaultServiceFee;
            decimal hTPrice = GetHTTotalPrice();

            return hTPrice + (CratesUsed * 5.95m) + serviceFee;
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
                total += line.GetSubTotal();
            return total;
        }

        public decimal TotalWithServiceFee()
        {
            return TotalAmount() + DefaultServiceFee;
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

        public static async Task<List<OrderDisplayViewModel>> GetAllOrdersAsync(IOrderDAL orderDAL, OrderStatus status, int? storeId, DateTime bookDate)
        {
            return await orderDAL.GetAllOrdersAsync(status, storeId, bookDate);
        }

        public static async Task<List<Order>> GetOrdersByClientAsync(IOrderDAL orderDAL, int clientId)
        {
            return await orderDAL.GetOrdersByClientAsync(clientId);
        }

        public static async Task<Order> GetOrderForChecklistAsync(IOrderDAL orderDAL, int orderId)
        {
            return await orderDAL.GetOrderForChecklistAsync(orderId);
        }
        
        public static async Task<Order> GetOrderForBillAsync(IOrderDAL orderDAL, int orderId)
        {
            return await orderDAL.GetOrderForBillAsync(orderId);
        }

        public async Task PlaceOrder(IOrderDAL orderDAL)
        {
            await orderDAL.CreateAsync(this);
        }

        public async Task<bool> UpdateCratesUsed(IOrderDAL orderDAL, int orderId, int cratesCount, int checkedProductsCount)
        {
            if(Lines.Count() == checkedProductsCount && cratesCount > 0)
            {
                return await orderDAL.UpdateCratesUsed(orderId, cratesCount);
            }
            return false;
        }
        
        public async Task<bool> UpdateCratesReturned(IOrderDAL orderDAL, int orderId, int cratesCount)
        {
            return await orderDAL.UpdateCratesReturned(orderId, cratesCount);
        }
    }
}
