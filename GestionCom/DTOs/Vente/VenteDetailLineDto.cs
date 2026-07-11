namespace GestionCom.DTOs.Vente
{
    public class VenteDetailLineDto
    {
        public int ProduitId { get; set; }
        public string ProduitLabel { get; set; } = null!;
        public int Quantite { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public decimal RemisePercent { get; set; }
        public decimal? PrixUnitaireApresRemise { get; set; }
        public decimal? TotalLigne { get; set; }
        public int CurrentStock { get; set; }
    }
}
