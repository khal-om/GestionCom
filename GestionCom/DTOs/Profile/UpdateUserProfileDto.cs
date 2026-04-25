using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Profile
{
    public class UpdateUserProfileDto
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email invalide.")]
        [StringLength(200)]
        public string? Email { get; set; }
    }
}
