using GestionCom.Data;
using GestionCom.DTOs.Dashboard;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class DashboardService
    {
        private static readonly string[] MonthLabels =
        [
            "Jan", "Fév", "Mar", "Avr", "Mai", "Juin",
            "Juil", "Août", "Sep", "Oct", "Nov", "Déc"
        ];

        private readonly AppDbContext _context;
        private readonly AppSettingsService _appSettingsService;

        public DashboardService(AppDbContext context, AppSettingsService appSettingsService)
        {
            _context = context;
            _appSettingsService = appSettingsService;
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync(
            int currentUserId,
            bool isAdmin,
            int? year = null,
            int? selectedUserId = null,
            int? monthFrom = null,
            int? monthTo = null,
            string? saleState = null)
        {
            var availableYears = await GetAvailableYearsAsync();
            var availableUsers = await GetAvailableUsersAsync();
            var selectedYear = ResolveSelectedYear(availableYears, year);
            var normalizedMonthFrom = NormalizeMonth(monthFrom, 1);
            var normalizedMonthTo = NormalizeMonth(monthTo, 12);

            if (normalizedMonthFrom > normalizedMonthTo)
            {
                (normalizedMonthFrom, normalizedMonthTo) = (normalizedMonthTo, normalizedMonthFrom);
            }

            var normalizedSaleState = NormalizeSaleState(saleState);
            var lowStockThreshold = await _appSettingsService.GetLowStockThresholdAsync();
            var effectiveSelectedUserId = isAdmin && selectedUserId.HasValue && availableUsers.Any(u => u.UserId == selectedUserId.Value)
                ? selectedUserId
                : null;
            var focusUserId = effectiveSelectedUserId ?? currentUserId;
            var focusUserName = availableUsers.FirstOrDefault(u => u.UserId == focusUserId)?.Username ?? "Moi";
            var yearStart = new DateTime(selectedYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = yearStart.AddYears(1);
            var utcToday = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var accessibleUserId = isAdmin ? (int?)null : currentUserId;

            var currentUserSalesQuery = _context.Ventes
                .AsNoTracking()
                .Where(v => v.CreatedBy == focusUserId && v.DateVente >= yearStart && v.DateVente < yearEnd);

            var accessibleSalesQuery = _context.Ventes
                .AsNoTracking()
                .Where(v => v.DateVente >= yearStart && v.DateVente < yearEnd);

            if (accessibleUserId.HasValue)
            {
                accessibleSalesQuery = accessibleSalesQuery.Where(v => v.CreatedBy == accessibleUserId.Value);
            }

            currentUserSalesQuery = ApplyMonthRange(currentUserSalesQuery, normalizedMonthFrom, normalizedMonthTo);
            accessibleSalesQuery = ApplyMonthRange(accessibleSalesQuery, normalizedMonthFrom, normalizedMonthTo);
            currentUserSalesQuery = ApplySaleState(currentUserSalesQuery, normalizedSaleState);
            accessibleSalesQuery = ApplySaleState(accessibleSalesQuery, normalizedSaleState);

            var currentUserSales = await currentUserSalesQuery.ToListAsync();
            var companySales = await accessibleSalesQuery.ToListAsync();

            var purchasesQuery = _context.Achats
                .AsNoTracking()
                .Where(a => a.DateAchat >= yearStart && a.DateAchat < yearEnd);

            purchasesQuery = ApplyMonthRange(purchasesQuery, normalizedMonthFrom, normalizedMonthTo);
            var companyPurchases = await purchasesQuery.ToListAsync();

            var expensesQuery = _context.Depenses
                .AsNoTracking()
                .Where(d => d.DateDepense >= yearStart && d.DateDepense < yearEnd);

            expensesQuery = ApplyMonthRange(expensesQuery, normalizedMonthFrom, normalizedMonthTo);
            var companyExpenses = await expensesQuery.ToListAsync();

            var totalSalesAllTime = await _context.Ventes
                .AsNoTracking()
                .Where(v => !accessibleUserId.HasValue || v.CreatedBy == accessibleUserId.Value)
                .SumAsync(v => (decimal?)v.TotalAmount) ?? 0m;

            var totalPaymentsAllTime = await _context.Payments
                .AsNoTracking()
                .Where(p => p.IsReceived)
                .Where(p => !accessibleUserId.HasValue || p.CreatedBy == accessibleUserId.Value)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var totalPurchasesRemaining = await _context.Achats
                .AsNoTracking()
                .SumAsync(a => (decimal?)(a.TotalAmount - a.PaidAmount)) ?? 0m;

            var topClients = await accessibleSalesQuery
                .GroupBy(v => new { v.ClientId, v.Client.Name })
                .Select(g => new DashboardTopClientDto
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalAmount = g.Sum(v => v.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(5)
                .ToListAsync();

            var userSales = isAdmin
                ? await accessibleSalesQuery
                    .Where(v => v.CreatedBy.HasValue)
                    .GroupBy(v => new { UserId = v.CreatedBy!.Value })
                    .Select(g => new
                    {
                        g.Key.UserId,
                        TotalAmount = g.Sum(v => v.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(5)
                    .ToListAsync()
                : [];

            var selectedUserIds = userSales.Select(x => x.UserId).Distinct().ToList();
            var userNameMap = await _context.Users
                .AsNoTracking()
                .Where(u => selectedUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var topUsers = userSales
                .Select(x => new DashboardUserSalesDto
                {
                    UserId = x.UserId,
                    Username = userNameMap.TryGetValue(x.UserId, out var username) ? username : $"Utilisateur #{x.UserId}",
                    TotalAmount = x.TotalAmount
                })
                .ToList();

            var lowStockProducts = await _context.Produits
                .AsNoTracking()
                .Select(p => new DashboardStockAlertDto
                {
                    ProduitId = p.Id,
                    ProduitLabel = string.IsNullOrWhiteSpace(p.Marque)
                        ? p.Designation
                        : p.Marque + " - " + p.Designation,
                    Stock = p.StockMovements.Sum(m => (int?)m.Quantity) ?? 0
                })
                .Where(p => p.Stock <= lowStockThreshold)
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.ProduitLabel)
                .Take(6)
                .ToListAsync();

            var upcomingEventEntities = await _context.CalendarEvents
                .AsNoTracking()
                .Where(e => e.EventDate >= utcToday)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            var creatorIds = upcomingEventEntities
                .Where(e => e.CreatedBy.HasValue)
                .Select(e => e.CreatedBy!.Value)
                .Distinct()
                .ToList();

            var creatorNames = await _context.Users
                .AsNoTracking()
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var upcomingEvents = upcomingEventEntities
                .Select(e => new DashboardUpcomingEventDto
                {
                    EventId = e.Id,
                    EventDate = e.EventDate,
                    Description = e.Description,
                    CreatedByName = e.CreatedBy.HasValue && creatorNames.TryGetValue(e.CreatedBy.Value, out var username)
                        ? username
                        : "Système",
                    IsToday = e.EventDate.Date == utcToday.Date
                })
                .ToList();

            return new DashboardOverviewDto
            {
                SelectedYear = selectedYear,
                AvailableYears = availableYears,
                AvailableUsers = availableUsers,
                SelectedUserId = effectiveSelectedUserId,
                SelectedMonthFrom = normalizedMonthFrom,
                SelectedMonthTo = normalizedMonthTo,
                SelectedSaleState = normalizedSaleState,
                SelectedUserDisplayName = focusUserName,
                CurrentUserYearSales = currentUserSales.Sum(v => v.TotalAmount),
                CompanyYearSales = companySales.Sum(v => v.TotalAmount),
                CompanyYearPurchases = companyPurchases.Sum(a => a.TotalAmount),
                CompanyYearExpenses = companyExpenses.Sum(d => d.Montant),
                OutstandingClientBalance = totalSalesAllTime - totalPaymentsAllTime,
                OutstandingSupplierBalance = totalPurchasesRemaining,
                UpcomingEventCount = upcomingEvents.Count,
                LowStockProductCount = lowStockProducts.Count,
                CurrentUserSalesByMonth = BuildMonthlySeries(currentUserSales.Select(v => (v.DateVente.Month, v.TotalAmount))),
                CompanySalesByMonth = BuildMonthlySeries(companySales.Select(v => (v.DateVente.Month, v.TotalAmount))),
                CompanyPurchasesByMonth = BuildMonthlySeries(companyPurchases.Select(a => (a.DateAchat.Month, a.TotalAmount))),
                CompanyExpensesByMonth = BuildMonthlySeries(companyExpenses.Select(d => (d.DateDepense.Month, d.Montant))),
                TopClients = topClients,
                TopUsers = topUsers,
                LowStockProducts = lowStockProducts,
                UpcomingEvents = upcomingEvents
            };
        }

        private async Task<List<int>> GetAvailableYearsAsync()
        {
            var venteYears = await _context.Ventes
                .AsNoTracking()
                .Select(v => v.DateVente.Year)
                .Distinct()
                .ToListAsync();

            var achatYears = await _context.Achats
                .AsNoTracking()
                .Select(a => a.DateAchat.Year)
                .Distinct()
                .ToListAsync();

            var depenseYears = await _context.Depenses
                .AsNoTracking()
                .Select(d => d.DateDepense.Year)
                .Distinct()
                .ToListAsync();

            return venteYears
                .Concat(achatYears)
                .Concat(depenseYears)
                .Append(DateTime.UtcNow.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
        }

        private async Task<List<DashboardUserOptionDto>> GetAvailableUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new DashboardUserOptionDto
                {
                    UserId = u.Id,
                    Username = u.Username
                })
                .ToListAsync();
        }

        private static IQueryable<Vente> ApplySaleState(IQueryable<Vente> query, string saleState)
        {
            return saleState switch
            {
                "Delivered" => query.Where(v => v.Etat == VenteEtat.Livre),
                "Ordered" => query.Where(v => v.Etat == VenteEtat.Commande),
                _ => query
            };
        }

        private static IQueryable<Vente> ApplyMonthRange(IQueryable<Vente> query, int monthFrom, int monthTo)
            => query.Where(v => v.DateVente.Month >= monthFrom && v.DateVente.Month <= monthTo);

        private static IQueryable<Achat> ApplyMonthRange(IQueryable<Achat> query, int monthFrom, int monthTo)
            => query.Where(a => a.DateAchat.Month >= monthFrom && a.DateAchat.Month <= monthTo);

        private static IQueryable<Depense> ApplyMonthRange(IQueryable<Depense> query, int monthFrom, int monthTo)
            => query.Where(d => d.DateDepense.Month >= monthFrom && d.DateDepense.Month <= monthTo);

        private static int ResolveSelectedYear(List<int> availableYears, int? year)
        {
            if (year.HasValue && availableYears.Contains(year.Value))
            {
                return year.Value;
            }

            return availableYears.Count > 0 ? availableYears[0] : DateTime.UtcNow.Year;
        }

        private static int NormalizeMonth(int? month, int fallback)
        {
            if (!month.HasValue)
            {
                return fallback;
            }

            return Math.Clamp(month.Value, 1, 12);
        }

        private static string NormalizeSaleState(string? saleState)
        {
            return saleState switch
            {
                "Delivered" => "Delivered",
                "Ordered" => "Ordered",
                _ => "All"
            };
        }

        private static List<DashboardMonthlyValueDto> BuildMonthlySeries(IEnumerable<(int Month, decimal Value)> values)
        {
            var totals = values
                .GroupBy(v => v.Month)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

            var result = new List<DashboardMonthlyValueDto>(12);

            for (var month = 1; month <= 12; month++)
            {
                result.Add(new DashboardMonthlyValueDto
                {
                    Month = month,
                    Label = MonthLabels[month - 1],
                    Value = totals.TryGetValue(month, out var value) ? value : 0m
                });
            }

            return result;
        }
    }
}
