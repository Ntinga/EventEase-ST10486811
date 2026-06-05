namespace EventEase.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int EventTypeId { get; set; }
        public EventType? EventType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty ;
    }
}
