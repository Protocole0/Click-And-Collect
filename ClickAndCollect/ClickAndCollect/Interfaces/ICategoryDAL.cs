using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface ICategoryDAL
    {
        List<Category> GetAll();
    }
}
