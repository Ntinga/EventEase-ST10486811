namespace EventEase.ViewModels
{
    public class BookingViewModel
    {
        public int BookingID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
