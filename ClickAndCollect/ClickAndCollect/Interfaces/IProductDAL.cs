using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IProductDal
    {
        List<Product> GetByCategoryId(int categoryId);
        Product? GetById(int id);
    }
}
