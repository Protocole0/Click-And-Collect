using ClickAndCollect.Interfaces;
using ClickAndCollect.ViewModels;

namespace ClickAndCollect.Models
{
    public class Cashier : User, IEmployee
    {
        private Store _store;
        public Store Store { get => _store; set => _store = value; }

        public Cashier(int id, string email, Store store) : base(id, email)
        {
            _store = store;
        }
        
        public Cashier(string? user_type, int? id, string? email, Store store) : base(user_type, id, email)
        {
            UserType = user_type ?? string.Empty;
            Id = id ?? 0;
            Email = email ?? string.Empty;
            Store = store;
        }

        public async Task<List<OrderDisplayViewModel>> GetAllOrdersAsync(IOrderDAL orderDAL)
        {
            return await Order.GetAllOrdersAsync(orderDAL, OrderStatus.READY_FOR_PICKUP, Store.StoreId, DateTime.Today);
        }
    }
}
