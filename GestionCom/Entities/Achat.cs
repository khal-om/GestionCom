namespace GestionCom.Entities
{
    public class Achat : BaseEntity
    {
        public string Fournisseur { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public DateTime DateAchat { get; set; }

        public string? Note { get; set; }

        public ICollection<AchatLigne> Lignes { get; set; } = new List<AchatLigne>();
    }
}
