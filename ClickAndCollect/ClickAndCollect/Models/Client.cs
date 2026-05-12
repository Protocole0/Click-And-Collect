using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClickAndCollect.Models
{
    public class Client
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        private string _email;

        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        private string _password;

        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 12, ErrorMessage = "Le mot de passe doit faire entre 12 et 20 caractères.")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public Client(int id, string firstname, string lastname, string email, string password)
        {
            Id = id;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Password = password;
        }

    }
}
