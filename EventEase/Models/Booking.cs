namespace EventEase.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int VenueID { get; set; }
        public int EventID { get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }

        public virtual Venue? Venue {  get; set; }

        public virtual Event? Event { get; set; }

    }
}
