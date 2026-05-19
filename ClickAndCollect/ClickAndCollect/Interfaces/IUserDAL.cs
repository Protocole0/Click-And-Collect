using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IUserDAL
    {
        Task<Client?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task CreateAsync(Client client);
    }
}
