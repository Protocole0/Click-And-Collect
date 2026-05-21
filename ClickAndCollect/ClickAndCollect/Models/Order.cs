using ClickAndCollect.Interfaces;
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

        private int _id;
        public int Id { 
            get => _id;
            init
            {
                if (value < 0)
                    throw new ArgumentException("L'id d'une commande ne peut être négatif");
                _id = value;
            }
        }

        private DateTime _orderDate;
        public DateTime OrderDate { 
            get => _orderDate; 
            set
            {
                if (value.Date > DateTime.Today)
                    throw new ArgumentException("La date où le client a passé commande ne peut être dans le futur.");
            }
        }

        private int _cratesUsed;
        public int CratesUsed { 
            get => _cratesUsed; 
            set
            {
                if (CratesUsed < 0)
                    throw new Exception("Le nombre de caisses utilisées ne peut être inférieure à 0.");
                _cratesUsed = value; 
            }
        }

        private int _cratesReturned;
        public int CratesReturned { 
            get => _cratesReturned;
            set
            {
                if (CratesReturned < 0)
                    throw new Exception("Le nombre de caisses rendues ne peut être négatif.");
                _cratesReturned = value;
            }
        }

        private OrderStatus _status;
        public OrderStatus Status { 
            get => _status;
            set
            {
                // If the value is not included in the possible values of the Enum
                if (!Enum.IsDefined(typeof(OrderStatus), value))
                {
                    throw new ArgumentException("Le statut de commande fourni n'existe pas ou est invalide.");
                }
                _status = value;
            }
        }

        // -- Objects linked as Order properties

        // List of the ordelines for each product
        private List<OrderLine> _lines;
        public List<OrderLine> Lines
        {
            get => _lines;
            set 
            {
                ArgumentNullException.ThrowIfNull(value);
                _lines = value; 
            }
        }

        private Client? _client;
        public Client? Client {
            get => _client;
            set
            {
                _client = value;
            }
        }

        private Store? _store;
        public Store? Store {
            get => _store;
            set
            {
                _store = value;
            }
        }

        private TimeSlot? _slot;
        public TimeSlot? Slot { 
            get => _slot; 
            set
            {
                _slot = value;
            }
        }

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

        public Order(int orderId, int cratesUsed, Client client)
        {
            Id = orderId;
            CratesUsed = cratesUsed;
            Client = client;
            _lines = new List<OrderLine>();
        }

        // Constructors for the GetOrderForBill method
        // in the OrderDAL for the cashier data
        public Order(int orderId, int cratesUsed, Client client, Store store, TimeSlot slot)
        {
            Id = orderId;
            CratesUsed = cratesUsed;
            Client = client;
            Store = store;
            Slot = slot;
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

        // --- Méthodes de calculs ---

        public int TotalItems()
        {
            // Calculate the actual item quantity of the order
            int total = 0;
            foreach (OrderLine line in _lines)
                total += line.Quantity;

            return total;
        }

        public decimal TotalAmount()
        {
            // Calculate total price of an order without
            // including the crate fee and the service fee
            decimal total = 0;
            foreach (OrderLine line in _lines)
                total += line.GetSubTotal();
            return total;
        }

        public decimal TotalWithServiceFee()
            // Calculate total price of an order including
            // the price of products and the service fee
            => TotalAmount() + DefaultServiceFee;

        public decimal TTCTotalAmount()
            // Calculate total price of an order including
            // the price of products (GetHTTotalPrice), the crate fee and the service fee
            => TotalWithServiceFee() + (CratesUsed * 5.95m);
        

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

        public async Task<bool> UpdateCratesUsed(IOrderDAL orderDAL, int cratesUsed, int checkedProductsCount)
        {
            CratesUsed = cratesUsed;
            if (CratesUsed < 1)
                throw new Exception("Le nombre de caisses utilisées ne peut être inférieure à un. Pour la préparation d'une commande, vous avez besoin d'au moins une caisse.");
            // If all the products have been checked
            if(Lines.Count() == checkedProductsCount)
            {
                return await orderDAL.UpdateCratesUsed(Id, CratesUsed);
            }
            return false;
        }
        
        public async Task<bool> UpdateCratesReturned(IOrderDAL orderDAL, int cratesReturned)
        {
            CratesReturned = cratesReturned;
            return await orderDAL.UpdateCratesReturned(Id, CratesReturned);
        }
    }
}
