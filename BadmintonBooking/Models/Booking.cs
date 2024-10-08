using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BadmintonBooking.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("Court")]
        public int CourtId { get; set; }
        public Court Court { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; } // Booked, Available, etc.

        [ForeignKey("User")]  // This points to User
        public int UserId { get; set; }    // Use UserId for the foreign key
        public User User { get; set; }     // Referencing User, not Player
    }
}
