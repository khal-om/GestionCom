namespace GestionCom.DTOs.Dashboard
{
    public class DashboardUpcomingEventDto
    {
        public int EventId { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; } = null!;
        public string CreatedByName { get; set; } = null!;
        public bool IsToday { get; set; }
    }
}
