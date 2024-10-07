namespace BadmintonBooking.Models
{
    public class Court
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }

}
