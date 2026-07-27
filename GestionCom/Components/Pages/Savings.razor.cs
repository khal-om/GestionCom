using GestionCom.Entities;
using GestionCom.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using GestionCom.Data; // TODO: adjust to wherever your DbContext actually lives


namespace GestionCom.Components.Pages;

public partial class Savings
{
    // IMPORTANT: this is IDbContextFactory<T>, not the DbContext itself.
    // Interactive Server components live for the whole SignalR circuit (the
    // whole visit, not one request), so injecting a scoped DbContext directly
    // would mean one shared, long-lived context for the entire session —
    // not thread-safe and prone to stale tracked entities. The factory gives
    // each operation its own short-lived context instead. See SETUP.md.
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!; // TODO: rename to your real DbContext type
    [Inject] private IQuoteService QuoteService { get; set; } = default!;
    [Inject] private ILogger<Savings>? Logger { get; set; }

    private List<SavingsEntry> History { get; set; } = new();
    private List<FallingItem> FallingItems { get; } = new();

    private decimal? AmountInput { get; set; }
    private decimal MonthTotal { get; set; }
    private bool IsSaving { get; set; }

    private string? QuoteText { get; set; }
    private SavingsTier? QuoteTier { get; set; }
    private int QuoteVersion { get; set; }
    private int CelebrateVersion { get; set; }
    private int? LastSavedEntryId { get; set; }

    // ---- Test mode: only ever true in Debug builds. See region below. ----
#if DEBUG
    private const bool IsTestMode = true;
#else
    private const bool IsTestMode = false;
#endif

    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        await ReloadHistoryAsync(db);
    }

    /// <summary>
    /// Creates the entry for the effective date (today, unless a test-mode
    /// date override is set), or overwrites it if that day already has a
    /// value and he wants to correct it. One entry per calendar day,
    /// enforced by the unique index on Date in the DbContext.
    /// </summary>
    private async Task HandleSaveAsync()
    {
        if (IsSaving) return;
        IsSaving = true;
        ErrorText = null;

        try
        {
            var amount = AmountInput.GetValueOrDefault(0m);
            if (amount < 0) amount = 0;

            await using var db = await DbFactory.CreateDbContextAsync();

            var date = GetEffectiveDate();
            var entry = await db.SavingsEntries.FirstOrDefaultAsync(e => e.Date == date);

            if (entry is null)
            {
                entry = new SavingsEntry { Date = date, AmountSaved = amount, CreatedAt = DateTime.UtcNow };
                db.SavingsEntries.Add(entry);
            }
            else
            {
                entry.AmountSaved = amount;
            }

            await db.SaveChangesAsync();

            // Re-sort rather than assume "newest" == "just saved" — in test mode
            // the saved date might not actually be the latest one in history.
            var existingIndex = History.FindIndex(e => e.Date == entry.Date);
            if (existingIndex >= 0) History.RemoveAt(existingIndex);
            History.Add(entry);
            History = History.OrderByDescending(e => e.Date).Take(30).ToList();

            LastSavedEntryId = entry.Id;
            MonthTotal = await GetMonthTotalAsync(db);

            var (quote, tier) = QuoteService.GetQuote(amount);
            QuoteText = quote;
            QuoteTier = tier;
            QuoteVersion++;     // changing @key forces the quote box to remount, replaying its fade-in
            CelebrateVersion++; // same trick for the button's little celebration wiggle

            _ = SpawnFallingItemsAsync(tier); // fire-and-forget: purely decorative, shouldn't block the UI
        }
        catch (Exception ex)
        {
            ErrorText = "Fehler beim Speichern. Bitte Konsole/Logs prüfen.";
            Logger?.LogError(ex, "Error saving savings entry");
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReloadHistoryAsync(AppDbContext db)
    {
        History = await db.SavingsEntries
            .OrderByDescending(e => e.Date)
            .Take(30)
            .ToListAsync();

        MonthTotal = await GetMonthTotalAsync(db);
    }

    private DateOnly GetEffectiveDate()
    {
        if (IsTestMode && TestDateOverride.HasValue)
            return TestDateOverride.Value;

        return DateOnly.FromDateTime(DateTime.Now);
    }

    private async Task<decimal> GetMonthTotalAsync(AppDbContext db)
    {
        var today = GetEffectiveDate();
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        return await db.SavingsEntries
            .Where(e => e.Date >= monthStart)
            .SumAsync(e => (decimal?)e.AmountSaved) ?? 0m;
    }

    #region Test mode — date override & sample data (Debug builds only)

    // These fields exist regardless of build config (simplest to compile),
    // but IsTestMode gates every place they can actually take effect, and
    // the markup only renders the panel that lets you set them when
    // IsTestMode is true. In a Release build, TestDateOverride can never be
    // set by anyone, so GetEffectiveDate() always falls back to "today".

    private DateOnly? TestDateOverride { get; set; }
    private int SeedDayCount { get; set; } = 14;
    private bool IsSeeding { get; set; }
    private string? ErrorText { get; set; }

    /// <summary>
    /// Generates SeedDayCount days of plausible-looking sample entries,
    /// counting back from the effective date, so you can preview weeks of
    /// history without clicking "Save it" dozens of times. Upserts, so
    /// running it again is safe and just refreshes the same date range.
    /// </summary>
    private async Task SeedSampleDataAsync()
    {
        if (!IsTestMode || IsSeeding) return;
        IsSeeding = true;
        await InvokeAsync(StateHasChanged);

        var random = Random.Shared;
        var endDate = GetEffectiveDate();

        await using var db = await DbFactory.CreateDbContextAsync();

        for (var i = 0; i < SeedDayCount; i++)
        {
            var date = endDate.AddDays(-i);
            var amount = GenerateSampleAmount(random);

            var entry = await db.SavingsEntries.FirstOrDefaultAsync(e => e.Date == date);
            if (entry is null)
            {
                db.SavingsEntries.Add(new SavingsEntry { Date = date, AmountSaved = amount, CreatedAt = DateTime.UtcNow });
            }
            else
            {
                entry.AmountSaved = amount;
            }
        }

        await db.SaveChangesAsync();
        await ReloadHistoryAsync(db);

        IsSeeding = false;
        await InvokeAsync(StateHasChanged);
    }

    private static decimal GenerateSampleAmount(Random random)
    {
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.15 => 0m,                                                    // occasional slip, for realism
            < 0.55 => Math.Round((decimal)(random.NextDouble() * 5), 2),      // low
            < 0.85 => Math.Round((decimal)(5 + random.NextDouble() * 10), 2), // medium
            _ => Math.Round((decimal)(15 + random.NextDouble() * 15), 2)      // high
        };
    }

    /// <summary>Wipes every entry so you can re-seed onto a clean slate.</summary>
    private async Task ClearAllDataAsync()
    {
        if (!IsTestMode) return;

        await using var db = await DbFactory.CreateDbContextAsync();
        await db.SavingsEntries.ExecuteDeleteAsync();

        History.Clear();
        MonthTotal = 0;
        LastSavedEntryId = null;
        QuoteText = null;
        QuoteTier = null;
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    private static string GetTierEmoji(SavingsTier? tier) => tier switch
    {
        SavingsTier.Zero => "🚬😏",
        SavingsTier.Low => "🙂",
        SavingsTier.Medium => "💪",
        SavingsTier.High => "🎉🚭",
        _ => "🙂"
    };

    private static string FormatTnd(decimal value)
    {
        // Tunisian formatting: 3 decimal places, comma as decimal separator in fr-TN/ar-TN cultures
        var culture = System.Globalization.CultureInfo.GetCultureInfo("fr-TN");
        return value.ToString("N3", culture) + " TND";
    }

    private static bool IsArabic(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (var ch in text)
        {
            // Arabic Unicode block ranges (basic and extended)
            if ((ch >= '\u0600' && ch <= '\u06FF') || (ch >= '\u0750' && ch <= '\u077F') || (ch >= '\u08A0' && ch <= '\u08FF'))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Spawns a batch of falling money (or confetti, for big wins) as plain
    /// component state — no JS interop needed. Each item gets randomized
    /// position/timing, renders as an absolutely-positioned span with an
    /// inline style, and the whole batch removes itself after the CSS
    /// animation has had time to finish.
    /// </summary>
    private async Task SpawnFallingItemsAsync(SavingsTier tier)
    {
        var random = Random.Shared;
        var batchId = Guid.NewGuid();
        var items = new List<FallingItem>();

        var moneyCount = tier switch
        {
            SavingsTier.Zero => 1,
            SavingsTier.Low => 3,
            SavingsTier.Medium => 6,
            SavingsTier.High => 10,
            _ => 3
        };

        var symbols = tier == SavingsTier.Zero ? new[] { "💨" } : new[] { "💶", "🪙" };

        for (var i = 0; i < moneyCount; i++)
        {
            items.Add(new FallingItem(
                Id: Guid.NewGuid(),
                BatchId: batchId,
                Symbol: symbols[random.Next(symbols.Length)],
                LeftPercent: random.NextDouble() * 100,
                DelaySeconds: random.NextDouble() * 0.3,
                DurationSeconds: 1.6,
                SizePx: 16 + random.NextDouble() * 16,
                IsConfetti: false,
                Color: null));
        }

        if (tier == SavingsTier.High)
        {
            string[] colors = { "#2EC4B6", "#FFB703", "#FF6B6B", "#F7F9FB" };
            for (var i = 0; i < 28; i++)
            {
                items.Add(new FallingItem(
                    Id: Guid.NewGuid(),
                    BatchId: batchId,
                    Symbol: "",
                    LeftPercent: random.NextDouble() * 100,
                    DelaySeconds: random.NextDouble() * 0.3,
                    DurationSeconds: 1.2 + random.NextDouble() * 0.7,
                    SizePx: 0,
                    IsConfetti: true,
                    Color: colors[random.Next(colors.Length)]));
            }
        }

        FallingItems.AddRange(items);
        await InvokeAsync(StateHasChanged);

        await Task.Delay(2200);
        FallingItems.RemoveAll(i => i.BatchId == batchId);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Formats a double for use inside an inline CSS "style" string.
    /// Deliberately invariant-culture: this is machine-readable CSS
    /// (e.g. "left:12.5%"), not a number shown to a person — if the
    /// server's culture is de-DE, plain ToString() would emit "12,5"
    /// and silently produce broken, ignored CSS.
    /// </summary>
    private static string FormatInvariant(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private sealed record FallingItem(
        Guid Id,
        Guid BatchId,
        string Symbol,
        double LeftPercent,
        double DelaySeconds,
        double DurationSeconds,
        double SizePx,
        bool IsConfetti,
        string? Color);
}
