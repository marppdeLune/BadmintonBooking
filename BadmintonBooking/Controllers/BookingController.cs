using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.Data;

namespace BadmintonBooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly BadmintonBookingContext _context;

        public BookingController(BadmintonBookingContext context)
        {
            _context = context;
        }

        // Index: List of available courts for booking
        public async Task<IActionResult> Index()
        {
            var courts = await _context.Courts.ToListAsync();
            var courtAvailability = new Dictionary<int, List<string>>();

            DateTime openingTime = DateTime.Today.AddHours(8);
            DateTime closingTime = DateTime.Today.AddHours(21);

            foreach (var court in courts)
            {
                var bookings = await _context.Bookings.Where(b => b.CourtId == court.CourtId).ToListAsync();
                var availableSlots = new List<string>();

                for (var time = openingTime; time < closingTime; time = time.AddHours(1))
                {
                    string timeSlot = time.ToString("hh:mm tt") + " - " + time.AddHours(1).ToString("hh:mm tt");

                    // Check if the court is booked for this specific time slot
                    bool isBooked = bookings.Any(b => b.StartTime <= time && b.EndTime > time);

                    if (!isBooked)
                    {
                        availableSlots.Add(timeSlot);
                    }
                }

                courtAvailability.Add(court.CourtId, availableSlots);
            }

            ViewBag.CourtAvailability = courtAvailability;
            return View(courts);
        }

        // POST: Book Court (Instant Booking from the Confirm Button)
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int courtId, string selectedTimeSlot)
        {
            // Retrieve the logged-in user's ID from the session
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Please log in to book a court.";
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid user. Please log in again.";
                return RedirectToAction("Login", "User");
            }

            var court = await _context.Courts.FindAsync(courtId);

            if (court == null)
            {
                TempData["ErrorMessage"] = "Invalid court selection.";
                return RedirectToAction("Index");
            }

            // Parse the selected time slot
            var timeRange = selectedTimeSlot?.Split(" - ");
            if (timeRange == null || timeRange.Length != 2)
            {
                TempData["ErrorMessage"] = "Invalid time slot format.";
                return RedirectToAction("Index");
            }

            DateTime startTime;
            DateTime endTime;

            // Parse the time range, ensuring correct formatting
            try
            {
                startTime = DateTime.Parse(timeRange[0].Trim());
                endTime = DateTime.Parse(timeRange[1].Trim());
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "Invalid time format.";
                return RedirectToAction("Index");
            }

            // Check if the selected time slot is already booked
            var existingBooking = await _context.Bookings
                .Where(b => b.CourtId == courtId && b.StartTime <= startTime && b.EndTime > startTime)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                TempData["ErrorMessage"] = "This time slot is already booked.";
                return RedirectToAction("Index");
            }

            // Create a new booking directly on button click
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

            TempData["SuccessMessage"] = "Court booked successfully!";
            return RedirectToAction("BookingSummary", new { bookingId = booking.BookingId });
        }

        // GET: Display Booking Summary
        public async Task<IActionResult> BookingSummary(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)  // Include User (could be Player)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: My Bookings (Shows the logged-in user's bookings)
        public async Task<IActionResult> MyBookings()
        {
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Please log in to view your bookings.";
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Where(b => b.UserId == user.UserId)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Edit Booking
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)  // Include User (could be Player)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Edit Booking
        [HttpPost]
        public async Task<IActionResult> Edit(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var existingBooking = await _context.Bookings.FindAsync(booking.BookingId);

            if (existingBooking == null)
            {
                return NotFound();
            }

            existingBooking.StartTime = booking.StartTime;
            existingBooking.EndTime = booking.EndTime;
            existingBooking.Price = booking.Price;

            _context.Bookings.Update(existingBooking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking updated successfully!";
            return RedirectToAction("BookingSummary", new { bookingId = booking.BookingId });
        }

        // GET: Confirm Delete
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)  // Include User (could be Player)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Confirmed Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking canceled successfully!";
            return RedirectToAction("Index");
        }
    }
}
