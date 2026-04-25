namespace GestionCom.DTOs.Dashboard
{
    public class DashboardOverviewDto
    {
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public List<DashboardUserOptionDto> AvailableUsers { get; set; } = new();

        public int? SelectedUserId { get; set; }
        public int SelectedMonthFrom { get; set; }
        public int SelectedMonthTo { get; set; }
        public string SelectedSaleState { get; set; } = "All";
        public string SelectedUserDisplayName { get; set; } = null!;

        public decimal CurrentUserYearSales { get; set; }
        public decimal CompanyYearSales { get; set; }
        public decimal CompanyYearPurchases { get; set; }
        public decimal CompanyYearExpenses { get; set; }
        public decimal OutstandingClientBalance { get; set; }
        public decimal OutstandingSupplierBalance { get; set; }
        public int UpcomingEventCount { get; set; }
        public int LowStockProductCount { get; set; }

        public List<DashboardMonthlyValueDto> CurrentUserSalesByMonth { get; set; } = new();
        public List<DashboardMonthlyValueDto> CompanySalesByMonth { get; set; } = new();
        public List<DashboardMonthlyValueDto> CompanyPurchasesByMonth { get; set; } = new();
        public List<DashboardMonthlyValueDto> CompanyExpensesByMonth { get; set; } = new();

        public List<DashboardTopClientDto> TopClients { get; set; } = new();
        public List<DashboardUserSalesDto> TopUsers { get; set; } = new();
        public List<DashboardStockAlertDto> LowStockProducts { get; set; } = new();
        public List<DashboardUpcomingEventDto> UpcomingEvents { get; set; } = new();
    }
}
