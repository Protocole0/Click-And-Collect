using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.ViewModels
{
    public class LoginViewModel
    {
        private string _email    = string.Empty;
        private string _password = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        public string Email
        {
            get => _email;
            set => _email = value;
        }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        public string Password
        {
            get => _password;
            set => _password = value;
        }
    }
}
