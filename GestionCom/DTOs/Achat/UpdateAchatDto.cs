using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Achat
{
    public class UpdateAchatDto : IValidatableObject
    {
        [Required(ErrorMessage = "Le fournisseur est requis.")]
        [StringLength(200)]
        public string Fournisseur { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public DateTime DateAchat { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        public List<CreateAchatLigneDto> Lignes { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TotalAmount <= 0)
            {
                yield return new ValidationResult("Le montant total doit être supérieur à 0.", [nameof(TotalAmount)]);
            }

            if (PaidAmount < 0)
            {
                yield return new ValidationResult("Le montant payé ne peut pas être négatif.", [nameof(PaidAmount)]);
            }

            if (PaidAmount > TotalAmount)
            {
                yield return new ValidationResult("Le montant payé ne peut pas dépasser le montant total.", [nameof(PaidAmount)]);
            }
        }
    }
}
