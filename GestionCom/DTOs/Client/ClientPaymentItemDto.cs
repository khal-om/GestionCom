namespace GestionCom.DTOs.Client
{
    public class ClientPaymentItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = null!;
        public string? PaymentNumber { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Label { get; set; }
        public bool IsReceived { get; set; }
        public string? Note { get; set; }
        // Optional link to a Vente this payment is applied to
        public int? VenteId { get; set; }
        public string? VenteRef { get; set; }
    }
}
