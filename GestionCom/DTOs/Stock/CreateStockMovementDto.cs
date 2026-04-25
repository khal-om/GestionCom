using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Stock
{
    public class CreateStockMovementDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Le produit est requis.")]
        public int ProduitId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0.")]
        public int Quantity { get; set; }
    }
}
