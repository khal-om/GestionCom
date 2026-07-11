namespace GestionCom.DTOs.Vente
{
    public class VentePaymentItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = null!;
        public string? PaymentNumber { get; set; }
        public bool IsReceived { get; set; }
        public string? Note { get; set; }
        public int ClientId { get; set; }
    }
}
