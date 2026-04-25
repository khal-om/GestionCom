namespace GestionCom.DTOs.Calendrier
{
    public class CalendarOverviewDto
    {
        public List<CalendarEventItemDto> UpcomingEvents { get; set; } = new();
        public List<CalendarEventItemDto> PastEvents { get; set; } = new();
    }
}
