using ClickAndCollect.ViewModels;

namespace ClickAndCollect.Interfaces
{
    public interface IEmployee
    {
        Task<List<OrderDisplayViewModel>> GetAllOrdersAsync(IOrderDAL orderDAL);
    }
}
