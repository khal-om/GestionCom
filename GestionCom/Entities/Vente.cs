namespace GestionCom.Entities
{
    public class Vente : BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public DateTime DateVente { get; set; }
        public DateTime? DateLivraison { get; set; }

        public VenteEtat Etat { get; set; } 

        public string? Note { get; set; }

        public ICollection<VenteLigne> Lignes { get; set; } = new List<VenteLigne>();
    }

    public enum VenteEtat
    {
        Commande = 0,
        Livre = 1
    }
}
