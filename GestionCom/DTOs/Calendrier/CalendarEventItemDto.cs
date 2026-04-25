namespace GestionCom.DTOs.Calendrier
{
    public class CalendarEventItemDto
    {
        public int Id { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; } = null!;
        public string CreatedByName { get; set; } = null!;
        public bool IsToday { get; set; }
        public bool IsPaymentReminder { get; set; }
        public string? ReminderBadge { get; set; }
    }
}
