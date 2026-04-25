namespace GestionCom.DTOs.Produit
{
    public class ProduitListItemDto
    {
        public int Id { get; set; }
        public string? Marque { get; set; }
        public string? Famille { get; set; }
        public string Designation { get; set; } = null!;
        public int Stock { get; set; }
    }
}
