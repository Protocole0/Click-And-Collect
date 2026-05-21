using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.ViewModels
{
    public class RegisterViewModel
    {
        private string _firstname = string.Empty;
        private string _lastname  = string.Empty;
        private string _email     = string.Empty;
        private string _phoneNumber    = string.Empty;
        private string _password       = string.Empty;
        private string _confirmPassword = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Le prénom doit faire entre 2 et 50 caractères.")]
        public string Firstname
        {
            get => _firstname;
            set => _firstname = value;
        }

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Le nom doit faire entre 2 et 50 caractères.")]
        public string Lastname
        {
            get => _lastname;
            set => _lastname = value;
        }

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères.")]
        public string Email
        {
            get => _email;
            set => _email = value;
        }

        [Required(ErrorMessage = "Le numéro de téléphone est requis.")]
        [StringLength(15, MinimumLength = 9, ErrorMessage = "Le numéro de téléphone doit faire entre 9 et 15 caractères.")]
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = value;
        }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 12, ErrorMessage = "Le mot de passe doit faire entre 12 et 20 caractères.")]
        public string Password
        {
            get => _password;
            set => _password = value;
        }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => _confirmPassword = value;
        }
    }
}
