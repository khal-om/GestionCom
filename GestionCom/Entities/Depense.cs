namespace GestionCom.Entities
{
    public class Depense : BaseEntity
    {
        public DateTime DateDepense { get; set; }

        public string Libelle { get; set; } = null!;

        public decimal Montant { get; set; }

        public string Description { get; set; } = null!;
    }
}
