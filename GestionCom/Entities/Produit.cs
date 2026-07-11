namespace GestionCom.Entities
{
    public class Produit : SoftDeletableEntity
    {
        public string? Marque { get; set; }
        public string? Famille { get; set; }
        public string Designation { get; set; } = null!;
        public decimal? PrixUnitaire { get; set; }

        public ICollection<AchatLigne> AchatLignes { get; set; } = new List<AchatLigne>();
        public ICollection<VenteLigne> VenteLignes { get; set; } = new List<VenteLigne>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
