namespace GestionCom.DTOs.Achat
{
    public class AchatListItemDto
    {
        public int Id { get; set; }
        public string Fournisseur { get; set; } = null!;
        public DateTime DateAchat { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int LineCount { get; set; }
    }
}
