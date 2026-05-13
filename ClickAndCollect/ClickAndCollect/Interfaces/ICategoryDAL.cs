using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface ICategoryDal
    {
        List<Category> GetAll();
    }
}
