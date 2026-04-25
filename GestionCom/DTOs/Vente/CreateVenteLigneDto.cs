using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Vente
{
    public class CreateVenteLigneDto
    {
        [Range(1, int.MaxValue)]
        public int ProduitId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0.")]
        public int Quantite { get; set; }
    }
}
