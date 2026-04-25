namespace GestionCom.DTOs.Achat
{
    public class AchatDetailLineDto
    {
        public int ProduitId { get; set; }
        public string ProduitLabel { get; set; } = null!;
        public int Quantite { get; set; }
        public int CurrentStock { get; set; }
    }
}
