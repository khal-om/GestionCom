using System.ComponentModel.DataAnnotations;

namespace GestionCom.DTOs.Stock
{
    public class CreateStockMovementDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Le produit est requis.")]
        public int ProduitId { get; set; }

        // Allow positive or negative quantities for manual stock adjustments.
        public int Quantity { get; set; }
    }
}
