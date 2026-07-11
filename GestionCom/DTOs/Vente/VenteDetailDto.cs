namespace GestionCom.DTOs.Vente
{
    public class VenteDetailDto
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public string? FactureNumber { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public string? ClientEmail { get; set; }
        public string? ClientTelephone { get; set; }
        public string? ClientPortable { get; set; }
        public string? ClientMatriculeFiscale { get; set; }
        public string? ClientAddress { get; set; }
        public string? ClientPostalCode { get; set; }
        public string? ClientCity { get; set; }
        public string? ClientCountry { get; set; }
        public DateTime DateVente { get; set; }
        public DateTime? DateLivraison { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Remaining { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string Etat { get; set; } = null!;
        public string? Note { get; set; }
        public List<VenteDetailLineDto> Lignes { get; set; } = new();
        public List<VentePaymentItemDto> Payments { get; set; } = new();
    }
}
