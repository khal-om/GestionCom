namespace GestionCom.DTOs.Dashboard
{
    public class DashboardUserSalesDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
