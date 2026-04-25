using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Profile
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Le nouveau mot de passe est requis.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 6 caractères.")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
