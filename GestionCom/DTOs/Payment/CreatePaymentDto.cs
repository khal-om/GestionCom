using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Payment
{
    public class CreatePaymentDto : IValidatableObject
    {
        public int ClientId { get; set; }

        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Le mode de paiement est requis.")]
        public string PaymentMode { get; set; } = null!;

        [StringLength(120)]
        public string? PaymentNumber { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(200)]
        public string? Label { get; set; }

        public bool IsReceived { get; set; } = true;

        [StringLength(500)]
        public string? Note { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount <= 0)
            {
                yield return new ValidationResult(
                    "Le montant doit être supérieur à 0.",
                    [nameof(Amount)]);
            }

            if (!PaymentModeOption.All.Contains(PaymentMode))
            {
                yield return new ValidationResult(
                    "Le mode de paiement est invalide.",
                    [nameof(PaymentMode)]);
            }

            if (PaymentMode is PaymentModeOption.Cheque or PaymentModeOption.Traite)
            {
                if (string.IsNullOrWhiteSpace(PaymentNumber))
                {
                    yield return new ValidationResult(
                        "Le numéro du règlement est requis.",
                        [nameof(PaymentNumber)]);
                }

                if (!PaymentDate.HasValue)
                {
                    yield return new ValidationResult(
                        "La date règlement est requise.",
                        [nameof(PaymentDate)]);
                }

                if (!DueDate.HasValue)
                {
                    yield return new ValidationResult(
                        "La date d'échéance est requise.",
                        [nameof(DueDate)]);
                }
            }
        }
    }
}
