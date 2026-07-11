namespace GestionCom.DTOs.Client
{
    public class ClientVenteItemDto
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public DateTime DateVente { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentStatus { get; set; } = null!;
        // Remaining amount to pay (TTC)
        public decimal Remaining => TotalAmount - PaidAmount;
    }
}
