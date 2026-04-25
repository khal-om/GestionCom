namespace GestionCom.DTOs.Dashboard
{
    public class DashboardStockAlertDto
    {
        public int ProduitId { get; set; }
        public string ProduitLabel { get; set; } = null!;
        public int Stock { get; set; }
    }
}
