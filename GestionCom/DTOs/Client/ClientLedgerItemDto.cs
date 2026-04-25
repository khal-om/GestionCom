namespace GestionCom.DTOs.Client
{
    public class ClientLedgerItemDto
    {
        public DateTime Date { get; set; }
        public string Objet { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int? ReferenceId { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}
