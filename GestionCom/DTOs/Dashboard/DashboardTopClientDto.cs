namespace GestionCom.DTOs.Dashboard
{
    public class DashboardTopClientDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
