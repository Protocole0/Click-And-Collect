using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IUserDAL
    {
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task CreateAsync(Client client);
    }
}
