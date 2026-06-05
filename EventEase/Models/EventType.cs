namespace EventEase.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }
        public string? Name { get; set; }
        public ICollection<Event>? Event { get; set; }
    }
}
