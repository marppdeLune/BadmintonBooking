using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.Data;

namespace BadmintonBooking.Controllers
{
    public class ReceptionistBookingController : Controller
    {
        private readonly BadmintonBookingContext _context;

        public ReceptionistBookingController(BadmintonBookingContext context)
        {
            _context = context;
        }

        // Index: List of available courts for booking
        public async Task<IActionResult> Index(DateTime? bookingDate)
        {
            var courts = await _context.Courts.ToListAsync();
            var courtAvailability = new Dictionary<int, Dictionary<string, string>>(); // Store slot statuses

            DateTime selectedDate = bookingDate ?? DateTime.Today;
            DateTime openingTime = selectedDate.AddHours(8);
            DateTime closingTime = selectedDate.AddHours(21);

            foreach (var court in courts)
            {
                var bookings = await _context.Bookings
                    .Where(b => b.CourtId == court.CourtId && b.StartTime.Date == selectedDate.Date)
                    .ToListAsync();

                var slotStatuses = new Dictionary<string, string>(); // Slot statuses for the court
                for (var time = openingTime; time < closingTime; time = time.AddHours(1))
                {
                    string timeSlot = time.ToString("hh:mm tt") + " - " + time.AddHours(1).ToString("hh:mm tt");

                    // Check if the court is booked for this specific time slot
                    bool isBooked = bookings.Any(b => b.StartTime <= time && b.EndTime > time);

                    // Check if the time slot is in the past for the current day
                    bool isInThePast = selectedDate.Date == DateTime.Now.Date && time < DateTime.Now;

                    if (isBooked)
                    {
                        slotStatuses.Add(timeSlot, "booked"); // Mark as booked
                    }
                    else if (isInThePast)
                    {
                        slotStatuses.Add(timeSlot, "unavailable"); // Mark as unavailable (past)
                    }
                    else
                    {
                        slotStatuses.Add(timeSlot, "available"); // Mark as available
                    }
                }

                courtAvailability.Add(court.CourtId, slotStatuses);
            }

            ViewBag.CourtAvailability = courtAvailability;
            ViewBag.SelectedDate = selectedDate;
            return View(courts);
        }

        // GET: Display Booking Preview for Receptionist
        [HttpGet]
        public async Task<IActionResult> BookingPreviewForReceptionist(int courtId, string selectedTimeSlot, DateTime bookingDate)
        {
            // Retrieve the selected court
            var court = await _context.Courts.FindAsync(courtId);
            if (court == null)
            {
                TempData["ErrorMessage"] = "Invalid court selection.";
                return RedirectToAction("Index");
            }

            // Split the selected time slot to get start and end times
            var timeRange = selectedTimeSlot?.Split(" - ");
            if (timeRange == null || timeRange.Length != 2)
            {
                TempData["ErrorMessage"] = "Invalid time slot format.";
                return RedirectToAction("Index");
            }

            DateTime startTime;
            DateTime endTime;

            try
            {
                startTime = bookingDate.Date.Add(DateTime.Parse(timeRange[0].Trim()).TimeOfDay);
                endTime = bookingDate.Date.Add(DateTime.Parse(timeRange[1].Trim()).TimeOfDay);
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "Invalid time format.";
                return RedirectToAction("Index");
            }

            // Create a temporary booking model to pass to the view for preview
            var bookingPreview = new Booking
            {
                CourtId = court.CourtId,
                Court = court,
                StartTime = startTime,
                EndTime = endTime,
                Price = court.Price
            };

            return View(bookingPreview);
        }

        // POST: Receptionist confirms the booking for a Player
        [HttpPost]
        public async Task<IActionResult> ConfirmBookingForPlayer(int courtId, string selectedTimeSlot, DateTime bookingDate, string playerUsername)
        {
            // Retrieve the player based on the entered username
            var player = await _context.Users.OfType<Player>().FirstOrDefaultAsync(u => u.Username == playerUsername);

            if (player == null)
            {
                TempData["ErrorMessage"] = $"Player with username '{playerUsername}' does not exist.";
                return RedirectToAction("BookingPreviewForReceptionist", new { courtId, selectedTimeSlot, bookingDate });
            }

            // Retrieve the selected court
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

            try
            {
                startTime = bookingDate.Date.Add(DateTime.Parse(timeRange[0].Trim()).TimeOfDay);
                endTime = bookingDate.Date.Add(DateTime.Parse(timeRange[1].Trim()).TimeOfDay);
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

            // Create the booking for the player
            var booking = new Booking
            {
                CourtId = court.CourtId,
                UserId = player.UserId,
                StartTime = startTime,
                EndTime = endTime,
                Price = court.Price,
                Status = "Booked"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Court booked successfully for player '{player.Username}'!";
            return RedirectToAction("BookingSummaryAsReceptionist", new { bookingId = booking.BookingId });
        }

        // GET: Booking Summary for Receptionist
        public async Task<IActionResult> BookingSummaryAsReceptionist(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User) // Retrieve the player info
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // Display the booking summary for a receptionist, including player info
        public async Task<IActionResult> BookingSummaryForReceptionist(int courtId, string selectedTimeSlot, DateTime bookingDate)
        {
            // Parse time slot
            var timeRange = selectedTimeSlot?.Split(" - ");
            if (timeRange == null || timeRange.Length != 2)
            {
                TempData["ErrorMessage"] = "Invalid time slot format.";
                return RedirectToAction("Index");
            }

            DateTime startTime = bookingDate.Date.Add(DateTime.Parse(timeRange[0].Trim()).TimeOfDay);

            // Retrieve the booking for the selected court, time, and date
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .FirstOrDefaultAsync(b => b.CourtId == courtId && b.StartTime == startTime);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "No booking found for the selected time slot.";
                return RedirectToAction("Index");
            }

            return View(booking); // Pass booking info to view
        }

        // POST: Delete a booking as a receptionist
        [HttpPost]
        public async Task<IActionResult> ConfirmDeleteBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
