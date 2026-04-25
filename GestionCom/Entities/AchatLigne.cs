namespace GestionCom.Entities
{
    public class AchatLigne : BaseEntity
    {
        public int AchatId { get; set; }
        public Achat Achat { get; set; } = null!;

        public int ProduitId { get; set; }
        public Produit Produit { get; set; } = null!;

        public int Quantite { get; set; }
    }
}
