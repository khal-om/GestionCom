namespace GestionCom.DTOs.Vente
{
    public class VenteDetailLineDto
    {
        public int ProduitId { get; set; }
        public string ProduitLabel { get; set; } = null!;
        public int Quantite { get; set; }
        public int CurrentStock { get; set; }
    }
}
