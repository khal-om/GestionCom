using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Calendrier
{
    public class CreateCalendarEventDto : IValidatableObject
    {
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "La description est requise.")]
        [StringLength(300)]
        public string Description { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EventDate == default)
            {
                yield return new ValidationResult("La date est requise.", [nameof(EventDate)]);
            }
        }
    }
}
