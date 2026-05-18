using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IStoreDAL
    {
        Task<List<Store>> GetAllAsync();
        Task<Store?> GetByIdAsync(int storeId);
    }
}
