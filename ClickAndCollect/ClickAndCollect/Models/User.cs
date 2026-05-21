using ClickAndCollect.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.Models
{
    abstract public class User
    {
        private int _id;
        public int Id
        {
            get => _id;
            init
            {
                if (value < 0)
                    throw new ArgumentException("L'id d'un utilisateur ne peut être négatif");
                _id = value;
            }
        }

        private string _email;
        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get { return _email; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("L'email du client ne peut être vide.");
                _email = value;
            }
        }

        private string _password;
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 12, ErrorMessage = "Le mot de passe doit faire entre 12 et 20 caractères.")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string UserType { get; set; } = string.Empty;

        protected User()
        {

        }
        public User(int id)
        {
            Id = id;
        }

        public User(int id, string email)
        {
            Id = id;
            Email = email;
        }

        public User(int id, string email, string password)
        {
            Id = id;
            Email = email;
            Password = password;
        }

        public User(string? user_type, int? id, string? email)
        {
            UserType = user_type ?? string.Empty;
            Id = id ?? 0;
            Email = email ?? string.Empty;
        }


        // --- Static method : the class delegates to the DAL ---

        public static async Task<User?> Login(string email, string password, IUserDAL userDAL)
            => await userDAL.GetByEmailAndPasswordAsync(email, password);
    }
}
