using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface ICategoryDAL
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int categoryId);
    }
}
