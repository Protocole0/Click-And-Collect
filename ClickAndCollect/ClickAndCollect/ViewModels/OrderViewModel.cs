using ClickAndCollect.Models;

namespace ClickAndCollect.ViewModels
{
    public class OrderViewModel
    {
        // Order
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus OrderStatus { get; set; }

        // Client
        public int ClientId { get; set; }
        public string ClientFirstname { get; set; }
        public string ClientLastname { get; set; }
        public string ClientPhoneNumber { get; set; }

        // OrderLine
        public int TotalItems { get; set; }

        // TimeSlot
        public DateTime BookDate { get; set; }
        public TimeSpan BookStart { get; set; }
        public TimeSpan BookEnd { get; set; }

        // Constructeur par défaut nécessaire quand on utilise
        // la syntaxe d'initialisation d'objet dans OrderDAL
        public OrderViewModel() {}

        public OrderViewModel(int orderId, DateTime orderDate, OrderStatus orderStatus, int clientId, string clientFirstname, string clientLastname, string clientPhoneNumber, int totalItems, DateTime bookDate, TimeSpan bookStart, TimeSpan bookEnd)
        {
            OrderId = orderId;
            OrderDate = orderDate;
            OrderStatus = orderStatus;
            ClientFirstname = clientFirstname;
            ClientLastname = clientLastname;
            ClientPhoneNumber = clientPhoneNumber;
            TotalItems = totalItems;
            BookDate = bookDate;
            BookStart = bookStart;
            BookEnd = bookEnd;
        }
    }
}
