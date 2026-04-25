namespace GestionCom.Entities
{
    public class Client : SoftDeletableEntity
    {
        // Section 1: Raison sociale
        public string Name { get; set; } = null!;
        public string? Profession { get; set; }
        public string? MatriculeFiscale { get; set; }

        // Section 2: Coordonnées
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Portable { get; set; }
        public string? Fax { get; set; }

        // Section 3: Adresse
        public string? Adresse { get; set; }
        public string? CodePostal { get; set; }
        public string? Ville { get; set; }
        public string? Pays { get; set; }

        // Section 4: Coordonnées bancaires
        public string? Banque { get; set; }
        public string? RIB { get; set; }

        // Note
        public string? Note { get; set; }

        // Navigation
        public ICollection<Vente> Ventes { get; set; } = new List<Vente>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
