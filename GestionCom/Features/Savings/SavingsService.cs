using Microsoft.EntityFrameworkCore;
using GestionCom.Data;
using GestionCom.Entities;
using System.Linq;

namespace GestionCom.Features.Savings;

public class SavingsService(AppDbContext db)
{
    public async Task<List<SavingEntry>> GetHistoryAsync(int take = 60) =>
        await db.SavingsEntries
            .OrderByDescending(e => e.Date)
            .Take(take)
            .Select(e => new SavingEntry { Id = e.Id, Date = e.Date, Amount = e.AmountSaved, CreatedAt = e.CreatedAt })
            .ToListAsync();

    public async Task<decimal> GetTotalAsync() =>
        await db.SavingsEntries.SumAsync(e => (decimal?)e.AmountSaved) ?? 0m;

    public async Task<SavingEntry?> GetTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var entity = await db.SavingsEntries.FirstOrDefaultAsync(e => e.Date == today);
        return entity is null ? null : new SavingEntry { Id = entity.Id, Date = entity.Date, Amount = entity.AmountSaved, CreatedAt = entity.CreatedAt };
    }

    public async Task<SavingEntry> SaveTodayAsync(decimal amount)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var existing = await db.SavingsEntries.FirstOrDefaultAsync(e => e.Date == today);

        if (existing is not null)
        {
            existing.AmountSaved = amount;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new SavingsEntry { Date = today, AmountSaved = amount, CreatedAt = DateTime.UtcNow };
            db.SavingsEntries.Add(existing);
        }

        await db.SaveChangesAsync();

        return new SavingEntry { Id = existing.Id, Date = existing.Date, Amount = existing.AmountSaved, CreatedAt = existing.CreatedAt };
    }
}
