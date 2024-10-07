namespace BadmintonBooking.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int CourtId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // Booked, Available, etc.
        public int UserId { get; set; }
    }

}
