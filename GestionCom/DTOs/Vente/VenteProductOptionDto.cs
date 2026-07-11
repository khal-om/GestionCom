namespace GestionCom.DTOs.Vente
{
    public class VenteProductOptionDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = null!;
        public int Stock { get; set; }
        public decimal? PrixUnitaire { get; set; }
    }
}
