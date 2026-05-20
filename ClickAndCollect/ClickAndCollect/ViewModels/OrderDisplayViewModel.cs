using ClickAndCollect.Models;
using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.ViewModels
{
    public class OrderDisplayViewModel
    {
        // Infos Order
        public int OrderId { get; set; }
        public int CratesUsed { get; set; }
        public OrderStatus Status { get; set; }

        // Infos Client
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string ClientPhoneNumber { get; set; }

        // Infos Time Slot
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime TimeSlotDate { get; set; }
        public TimeSpan TimeSlotStartTime { get; set; }
        public TimeSpan TimeSlotEndTime { get; set; }

        // Infos Order Lines
        public List<OrderLineDisplayViewModel> Lines { get; set; }

        // Constructor
        public OrderDisplayViewModel(int orderId, int cratesUsed, OrderStatus status, string clientFirstName, string clientLastName, string clientPhoneNumber, DateTime timeSlotDate, TimeSpan timeSlotStartTime, TimeSpan timeSlotEndTime)
        {
            OrderId = orderId;
            CratesUsed = cratesUsed;
            Status = status;
            ClientFirstName = clientFirstName;
            ClientLastName = clientLastName;
            ClientPhoneNumber = clientPhoneNumber;
            TimeSlotDate = timeSlotDate;
            TimeSlotStartTime = timeSlotStartTime;
            TimeSlotEndTime = timeSlotEndTime;
            Lines = new List<OrderLineDisplayViewModel>();
        }

        // -- Display methods in the view --
        public string GetClientFullName()
        {
            return $"{ClientFirstName} {ClientLastName}";
        }

        public int GetTotalItems()
        {
            // Using of lambda expression in the LINQ Sum method
            // to calculate total quantity of items in the order
            return Lines.Sum(l => l.Quantity);
        }

        public decimal GetHTTotalPrice()
        {
            // Using of lambda expression in the LINQ Sum method
            // to calculate total price of an order without
            // including the crate fee and the service fee
            return Lines.Sum(l => l.Quantity * l.ProductPrice);
        }
        
        public decimal GetTTCTotalPrice()
        {
            // Calculate total price of an order including
            // the price of products (GetHTTotalPrice), the crate fee and the service fee
            decimal serviceFee = Order.DefaultServiceFee;
            decimal hTPrice = GetHTTotalPrice();

            return hTPrice + (CratesUsed * 5.95m) + serviceFee;
        }

        public string GetTimeSlotLabel()
        {
            return $"{TimeSlotStartTime:hh\\:mm}-{TimeSlotEndTime:hh\\:mm}";
        }
    }
}
