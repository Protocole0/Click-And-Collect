using ClickAndCollect.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.Models
{
    public class Client : User
    {
        private string _firstname = string.Empty;
        public string Firstname
        {
            get => _firstname;
            set => _firstname = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException("Le prénom ne peut pas être vide.");
        }

        private string _lastname = string.Empty;
        public string Lastname
        {
            get => _lastname;
            set => _lastname = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException("Le nom ne peut pas être vide.");
        }

        public string PhoneNumber { get; set; } = string.Empty;

        public Client()
        {

        }

        public Client(int id, string firstname, string lastname, string email, string password, string phoneNumber): base(id, email, password)
        {
            Firstname = firstname;
            Lastname = lastname;
            PhoneNumber = phoneNumber;
        }

        // Constructor just to display the name of the client when his order has been prepared
        public Client(string firstname, string lastname)
        {
            Firstname = firstname;
            Lastname = lastname;
        }

        public Client(int id): base(id)
        {

        }

        public Client(int id, string firstname, string lastname, string phoneNumber) : base(id)
        {
            Firstname   = firstname;
            Lastname    = lastname;
            PhoneNumber = phoneNumber;
        }

        // --- Méthodes statiques : la classe délègue au DAL ---

        public static async Task<bool> EmailExists(string email, IUserDAL userDAL)
        {
            return await userDAL.EmailExistsAsync(email);
        }

        // --- Méthodes d'instance : l'objet Client communique avec le DAL ---

        public async Task CreateAccount(IUserDAL userDAL)
        {
            await userDAL.CreateAsync(this);
        }

        public async Task<List<Order>> GetOrdersAsync(IOrderDAL orderDAL)
        {
            return await orderDAL.GetOrdersByClientAsync(Id);
        }
    }
}
