namespace GestionCom.Entities
{
    public class StockMovement : BaseEntity
    {
        public int ProduitId { get; set; }
        public Produit Produit { get; set; } = null!;

        public int Quantity { get; set; }

        public string MovementType { get; set; } = null!;
        public int? ReferenceId { get; set; }
    }
}
