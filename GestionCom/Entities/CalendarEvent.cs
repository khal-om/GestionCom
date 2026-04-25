namespace GestionCom.Entities
{
    public class CalendarEvent : BaseEntity
    {
        public DateTime EventDate { get; set; }

        public string Description { get; set; } = null!;
    }
}
