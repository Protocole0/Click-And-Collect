using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IProductDal
    {
        Task<List<Product>> GetByCategoryIdAsync(int categoryId);

        Task<Product?> GetByIdAsync(int id);
    }
}
