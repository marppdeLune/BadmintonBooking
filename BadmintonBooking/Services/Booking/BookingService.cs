using BadmintonBooking.Data;
using BadmintonBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly BadmintonBookingContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingService(BadmintonBookingContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Dictionary<int, List<string>>> GetCourtAvailabilityAsync(DateTime? bookingDate)
        {
            var courts = await _context.Courts.ToListAsync();
            var courtAvailability = new Dictionary<int, List<string>>();
            DateTime selectedDate = bookingDate ?? DateTime.Today;
            DateTime openingTime = selectedDate.AddHours(8);
            DateTime closingTime = selectedDate.AddHours(21);

            foreach (var court in courts)
            {
                var bookings = await _context.Bookings
                    .Where(b => b.CourtId == court.CourtId && b.StartTime.Date == selectedDate.Date)
                    .ToListAsync();

                var availableSlots = new List<string>();
                for (var time = openingTime; time < closingTime; time = time.AddHours(1))
                {
                    string timeSlot = time.ToString("hh:mm tt") + " - " + time.AddHours(1).ToString("hh:mm tt");
                    // Check if the court is booked for this specific time slot
                    bool isBooked = bookings.Any(b => b.StartTime <= time && b.EndTime > time);
                    // Check if the time slot is in the past for the current day
                    bool isInThePast = selectedDate.Date == DateTime.Now.Date && time < DateTime.Now;

                    if (!isBooked && !isInThePast)
                    {
                        availableSlots.Add(timeSlot);
                    }
                }
                courtAvailability.Add(court.CourtId, availableSlots);
            }

            return courtAvailability;
        }

        public async Task<Booking> GetBookingPreviewAsync(int courtId, string selectedTimeSlot, DateTime bookingDate, string username)
        {
            // Get the user object based on the username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            // Get the selected court
            var court = await _context.Courts.FindAsync(courtId);

            if (user == null || court == null || string.IsNullOrEmpty(selectedTimeSlot))
            {
                return null;
            }
            
            // Split the selected time slot to extract start and end times
            var timeRange = selectedTimeSlot.Split(" - ");
            DateTime startTime = bookingDate.Date.Add(DateTime.Parse(timeRange[0].Trim()).TimeOfDay);
            DateTime endTime = bookingDate.Date.Add(DateTime.Parse(timeRange[1].Trim()).TimeOfDay);

            // Create a Booking model for the preview, but don't save it yet
            return new Booking
            {
                CourtId = courtId,
                Court = court,
                UserId = user.UserId,
                User = user,
                StartTime = startTime,
                EndTime = endTime,
                Price = court.Price,
                Status = "Pending" // Just for preview; status will be updated when confirmed
            };
        }

        public async Task<Booking> ConfirmBookingAsync(int courtId, string selectedTimeSlot, DateTime bookingDate, string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var court = await _context.Courts.FindAsync(courtId);

            if (user == null || court == null || string.IsNullOrEmpty(selectedTimeSlot))
            {
                return null;
            }
            
            // Parse the selected time slot
            var timeRange = selectedTimeSlot.Split(" - ");
            DateTime startTime = bookingDate.Date.Add(DateTime.Parse(timeRange[0].Trim()).TimeOfDay);
            DateTime endTime = bookingDate.Date.Add(DateTime.Parse(timeRange[1].Trim()).TimeOfDay);

            // Check if the selected time slot is already booked
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.CourtId == courtId && b.StartTime <= startTime && b.EndTime > startTime);

            if (existingBooking != null)
            {
                return null; // Time slot is already booked
            }

            // Create a new booking
            var booking = new Booking
            {
                CourtId = courtId,
                UserId = user.UserId,
                StartTime = startTime,
                EndTime = endTime,
                Price = court.Price,
                Status = "Booked"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return false;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Booking> GetBookingSummaryAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User) // Include User (could be Player)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<List<Booking>> GetMyBookingsAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            return user != null
                ? await _context.Bookings.Include(b => b.Court).Where(b => b.UserId == user.UserId).ToListAsync()
                : new List<Booking>();
        }
    }
}
