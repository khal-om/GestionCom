using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Client
{
    public class CreateClientDto
    {
        [Required(ErrorMessage = "Le nom du client est requis.")]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(150)]
        public string? Profession { get; set; }

        [StringLength(100)]
        public string? MatriculeFiscale { get; set; }

        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? Telephone { get; set; }

        [StringLength(50)]
        public string? Portable { get; set; }

        [StringLength(50)]
        public string? Fax { get; set; }

        [StringLength(300)]
        public string? Adresse { get; set; }

        [StringLength(20)]
        public string? CodePostal { get; set; }

        [StringLength(120)]
        public string? Ville { get; set; }

        [StringLength(120)]
        public string? Pays { get; set; }

        [StringLength(150)]
        public string? Banque { get; set; }

        [StringLength(100)]
        public string? RIB { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }
    }
}
