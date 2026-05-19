using ClickAndCollect.Models;

namespace ClickAndCollect.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string firstname, string lastname, string email);
        Task SendOrderConfirmationEmailAsync(string toEmail, string firstname, string lastname, Order order, Store store, TimeSlot slot);
    }
}
