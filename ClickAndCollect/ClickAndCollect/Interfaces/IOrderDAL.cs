using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IOrderDAL
    {
        Task<List<Order>> GetAllOrdersAsync(OrderStatus status);
        Task<List<Order>> GetOrdersByClientAsync(int clientId);
        Task<Order> GetOrderAsync(int orderId);
        Task CreateAsync(Order order);
    }
}
