using BadmintonBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonBooking.Data
{
    public class BadmintonBookingContext : DbContext
    {
        public BadmintonBookingContext(DbContextOptions<BadmintonBookingContext> options)
            : base(options)
        {
        }

        // Define DbSets for your entities
        public DbSet<User> Users { get; set; }
		public DbSet<Player> Players { get; set; }
		public DbSet<Court> Courts { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial court data
            modelBuilder.Entity<Court>().HasData(
                new Court { CourtId = 1, CourtName = "Court 1", Price = 25.00m},
                new Court { CourtId = 2, CourtName = "Court 2", Price = 25.00m},
                new Court { CourtId = 3, CourtName = "Court 3", Price = 25.00m},
                new Court { CourtId = 4, CourtName = "Court 4", Price = 25.00m},
                new Court { CourtId = 5, CourtName = "Court 5", Price = 25.00m},
                new Court { CourtId = 6, CourtName = "Court 6", Price = 25.00m},
                new Court { CourtId = 7, CourtName = "Court 7", Price = 25.00m},
                new Court { CourtId = 8, CourtName = "Court 8", Price = 25.00m},
                new Court { CourtId = 9, CourtName = "Court 9", Price = 25.00m},
                new Court { CourtId = 10, CourtName = "Court 10", Price = 25.00m}
            );
        }
    }
}
