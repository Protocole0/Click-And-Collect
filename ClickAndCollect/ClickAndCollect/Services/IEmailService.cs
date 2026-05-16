namespace ClickAndCollect.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string firstname, string lastname, string email);
    }
}
