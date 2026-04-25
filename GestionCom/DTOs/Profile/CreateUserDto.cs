using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Profile
{
    public class CreateUserDto : IValidatableObject
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email invalide.")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Le rôle est requis.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        public string ConfirmPassword { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                yield return new ValidationResult("La confirmation du mot de passe ne correspond pas.", [nameof(ConfirmPassword)]);
            }
        }
    }
}
