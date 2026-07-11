using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Vente
{
    public class CreateVenteLigneDto
    {
        [Range(1, int.MaxValue)]
        public int ProduitId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0.")]
        public int Quantite { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Le prix unitaire doit être supérieur ou égal à 0.")]
        public decimal? PrixUnitaire { get; set; }

        [Range(typeof(decimal), "0", "100", ErrorMessage = "La remise doit être comprise entre 0 et 100 %.")]
        public decimal RemisePercent { get; set; }
    }
}
