using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Settings
{
    public class AppSettingsDto
    {
        [Required(ErrorMessage = "Le nom de la société est requis.")]
        [StringLength(200)]
        public string CompanyName { get; set; } = "FedNad Holding";

        [StringLength(200)]
        public string? CompanyTagline { get; set; } = "Gestion commerciale";

        [StringLength(300)]
        public string? CompanyAddress { get; set; }

        [StringLength(50)]
        public string? CompanyPhone { get; set; }

        [EmailAddress(ErrorMessage = "Email invalide.")]
        [StringLength(200)]
        public string? SupportEmail { get; set; }

        [Range(0, 100, ErrorMessage = "Le seuil stock faible doit être entre 0 et 100.")]
        public int LowStockThreshold { get; set; } = 5;
    }
}
