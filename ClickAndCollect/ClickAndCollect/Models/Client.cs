using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClickAndCollect.Models
{
    public class Client : User
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string PhoneNumber { get; set; }

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
        
        // Constructor without the password parameter
        // To display non-sensible informations
        // Like the name in the order dashboard
        public Client(int id, string firstname, string lastname, string phoneNumber): base(id)
        {
            Firstname = firstname;
            Lastname = lastname;
            PhoneNumber = phoneNumber;
        }

    }
}
