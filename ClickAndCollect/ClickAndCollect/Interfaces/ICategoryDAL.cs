using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface ICategoryDal
    {
        Task<List<Category>> GetAllAsync();
    }
}
