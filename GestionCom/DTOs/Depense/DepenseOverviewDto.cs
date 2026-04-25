namespace GestionCom.DTOs.Depense
{
    public class DepenseOverviewDto
    {
        public List<DepenseListItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
