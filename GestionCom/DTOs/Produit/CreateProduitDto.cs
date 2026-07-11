using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Produit
{
    public class CreateProduitDto
    {
        [StringLength(120)]
        public string? Marque { get; set; }

        [StringLength(120)]
        public string? Famille { get; set; }

        [Required(ErrorMessage = "La désignation est requise.")]
        [StringLength(200)]
        public string Designation { get; set; } = null!;

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Le prix unitaire doit être supérieur ou égal à 0.")]
        public decimal? PrixUnitaire { get; set; }
    }
}
