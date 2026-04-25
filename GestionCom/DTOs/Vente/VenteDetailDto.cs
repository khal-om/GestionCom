namespace GestionCom.DTOs.Vente
{
    public class VenteDetailDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime DateVente { get; set; }
        public DateTime? DateLivraison { get; set; }
        public decimal TotalAmount { get; set; }
        public string Etat { get; set; } = null!;
        public string? Note { get; set; }
        public List<VenteDetailLineDto> Lignes { get; set; } = new();
    }
}
