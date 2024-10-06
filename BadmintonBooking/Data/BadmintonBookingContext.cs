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

		public DbSet<Item> Items { get; set; }
	}
}
