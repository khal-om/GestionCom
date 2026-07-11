namespace GestionCom.Entities
{
    public class VenteLigne : BaseEntity
    {
        public int VenteId { get; set; }
        public Vente Vente { get; set; } = null!;

        public int ProduitId { get; set; }
        public Produit Produit { get; set; } = null!;

        public int Quantite { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public decimal RemisePercent { get; set; }
    }
}
