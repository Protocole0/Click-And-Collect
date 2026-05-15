using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IProductDAL
    {
        List<Product> GetByCategoryId(int categoryId);
        Product? GetById(int id);
    }
}
