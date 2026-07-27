namespace GestionCom.Entities;

/// <summary>
/// One day's worth of "didn't smoke / didn't buy coffee" savings.
/// Deliberately minimal — this feature is about a daily smile, not record-keeping.
/// </summary>
public class SavingsEntry
{
    public int Id { get; set; }

    /// <summary>
    /// The calendar day this saving applies to (not when the row was written).
    /// There's a unique index on this in the DbContext so one day = one entry;
    /// re-saving the same day updates the existing row instead of stacking duplicates.
    /// </summary>
    public DateOnly Date { get; set; }

    public decimal AmountSaved { get; set; }

    /// <summary>
    /// When the row was first created, UTC. Not shown in the UI — kept only
    /// because "when was this actually written" is a near-free, genuinely
    /// useful thing to have if you ever need to debug or audit an entry.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
