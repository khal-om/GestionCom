namespace GestionCom.Entities
{
    public class Vente : BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public string DocumentNumber { get; set; } = null!;

        public string? FactureNumber { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime DateVente { get; set; }
        public DateTime? DateLivraison { get; set; }

        public VenteEtat Etat { get; set; } 

        // Paid amount applied to this vente (TTC)
        public decimal PaidAmount { get; set; }

        // Payment state of the vente: NonPayee, EnCours (partially), Payee (fully paid)
        public PaiementEtat PaiementEtat { get; set; }

        public string? Note { get; set; }

        public ICollection<VenteLigne> Lignes { get; set; } = new List<VenteLigne>();
    }

    public enum VenteEtat
    {
        Commande = 0,
        Livre = 1
    }

    public enum PaiementEtat
    {
        NonPayee = 0,
        EnCours = 1,
        Payee = 2
    }
}
