namespace GestionCom.DTOs.Dashboard
{
    public class DashboardMonthlyValueDto
    {
        public int Month { get; set; }
        public string Label { get; set; } = null!;
        public decimal Value { get; set; }
    }
}
