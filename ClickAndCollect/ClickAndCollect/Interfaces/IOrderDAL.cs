using ClickAndCollect.Models;
using ClickAndCollect.ViewModels;

namespace ClickAndCollect.Interfaces
{
    public interface IOrderDAL
    {
        Task<List<OrderViewModel>> GetAllOrdersAsync(OrderStatus status, int storeId);
        Task<List<Order>> GetOrdersByClientAsync(int clientId);
        Task<Order> GetOrderAsync(int orderId);
        Task<bool> UpdateCratesUsed(int orderId, int cratesCount, OrderStatus status);
        Task CreateAsync(Order order);
    }
}
