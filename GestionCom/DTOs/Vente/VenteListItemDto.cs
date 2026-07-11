namespace GestionCom.DTOs.Vente
{
    public class VenteListItemDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime DateVente { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        // Remaining amount to pay (TTC)
        public decimal Remaining { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string Etat { get; set; } = null!;
        public int LineCount { get; set; }
    }
}
