using ClickAndCollect.DAL;
using ClickAndCollect.Interfaces;
using ClickAndCollect.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ClickAndCollect.Models
{
    public class Client : User
    {
        private string _firstname;
        public string Firstname
        {
            get => _firstname;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le prénom du client ne peut être vide.");
                _firstname = value.Trim();
            }
        }

        private string _lastname;
        public string Lastname
        {
            get => _lastname;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le nom de famille du client ne peut être vide.");
                _lastname = value.Trim();
            }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = value;
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
        
        public Client(string firstname, string lastname, string email)
        {
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
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

        // --- Static methods : the class delegates to the DAL ---

        public static async Task<bool> EmailExists(string email, IUserDAL userDAL)
        {
            return await userDAL.EmailExistsAsync(email);
        }

        // --- Instance methods : the Client object communicates with the DAL ---

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
