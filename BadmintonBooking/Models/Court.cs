using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.Models
{
    public class Court
    {
        [Key]
        public int CourtId { get; set; }
        public string CourtName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }


}
