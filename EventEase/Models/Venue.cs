namespace EventEase.Models
{
    public class Venue
    {
        public int VenueID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string ImageURL { get; set; } = string.Empty;
    }
}
