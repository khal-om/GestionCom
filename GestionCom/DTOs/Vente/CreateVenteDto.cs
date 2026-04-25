using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Vente
{
    public class CreateVenteDto : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Le client est requis.")]
        public int ClientId { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime DateVente { get; set; }
        public DateTime? DateLivraison { get; set; }

        [Required]
        public string Etat { get; set; } = "Livre";

        [StringLength(1000)]
        public string? Note { get; set; }

        public List<CreateVenteLigneDto> Lignes { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TotalAmount <= 0)
            {
                yield return new ValidationResult(
                    "Le montant doit être supérieur à 0.",
                    [nameof(TotalAmount)]);
            }
        }
    }
}
