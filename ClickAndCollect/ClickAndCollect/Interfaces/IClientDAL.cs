using ClickAndCollect.Models;

namespace ClickAndCollect.Interfaces
{
    public interface IClientDAL
    {
        Task<Client?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task CreateAsync(Client client);
    }
}
