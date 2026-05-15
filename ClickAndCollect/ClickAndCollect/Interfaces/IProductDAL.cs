using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IProductDAL
    {
        Task<List<Product>> GetByCategoryIdAsync(int categoryId);

        Task<Product?> GetByIdAsync(int id);
    }
}
