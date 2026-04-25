namespace GestionCom.DTOs.Depense
{
    public class DepenseListItemDto
    {
        public int Id { get; set; }
        public DateTime DateDepense { get; set; }
        public string Libelle { get; set; } = null!;
        public decimal Montant { get; set; }
        public string Description { get; set; } = null!;
        public string CreatedByName { get; set; } = null!;
    }
}
