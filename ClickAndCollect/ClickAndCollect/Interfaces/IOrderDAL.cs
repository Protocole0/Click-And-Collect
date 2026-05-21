using ClickAndCollect.Models;
using ClickAndCollect.ViewModels;

namespace ClickAndCollect.Interfaces
{
    public interface IOrderDAL
    {
        Task<List<OrderDisplayViewModel>> GetAllOrdersAsync(OrderStatus status, int? storeId, DateTime bookDate);
        Task<List<Order>> GetOrdersByClientAsync(int clientId);
        Task<Order> GetOrderForChecklistAsync(int orderId);
        Task<Order> GetOrderForBillAsync(int orderId);
        Task<bool> UpdateCratesUsed(int orderId, int cratesCount);
        Task<bool> UpdateCratesReturned(int orderId, int cratesCount);
        Task CreateAsync(Order order);
    }
}
