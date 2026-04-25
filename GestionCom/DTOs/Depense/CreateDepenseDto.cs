using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Depense
{
    public class CreateDepenseDto : IValidatableObject
    {
        public DateTime DateDepense { get; set; }

        [Required(ErrorMessage = "Le libellé est requis.")]
        [StringLength(200)]
        public string Libelle { get; set; } = null!;

        public decimal Montant { get; set; }

        [Required(ErrorMessage = "La description est requise.")]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateDepense == default)
            {
                yield return new ValidationResult("La date est requise.", [nameof(DateDepense)]);
            }

            if (Montant <= 0)
            {
                yield return new ValidationResult("Le montant doit être supérieur à 0.", [nameof(Montant)]);
            }
        }
    }
}
