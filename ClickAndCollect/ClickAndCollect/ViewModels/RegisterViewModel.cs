using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le prénom est requis.")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string Lastname { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est requis.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 12, ErrorMessage = "Le mot de passe doit faire entre 12 et 20 caractères.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
