using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.Data;
using BadmintonBooking.Services;

namespace BadmintonBooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly BadmintonBookingContext _context;
        private readonly IBookingService _bookingService;

        public BookingController(BadmintonBookingContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        // Index: List of available courts for booking
        public async Task<IActionResult> Index(DateTime? bookingDate)
        {
            var courts = await _context.Courts.ToListAsync();
            var courtAvailability = await _bookingService.GetCourtAvailabilityAsync(bookingDate);

            ViewBag.CourtAvailability = courtAvailability;
            ViewBag.SelectedDate = bookingDate ?? DateTime.Today;

            return View(courts);
        }


        // GET: Display Booking Preview
        public async Task<IActionResult> BookingPreview(int courtId, string selectedTimeSlot, DateTime bookingDate)
        {
            // Retrieve the logged-in user's username from the session
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Please log in to preview the booking.";
                return RedirectToAction("Login", "User");
            }

            var bookingPreview = await _bookingService.GetBookingPreviewAsync(courtId, selectedTimeSlot, bookingDate, username);

            if (bookingPreview == null)
            {
                TempData["ErrorMessage"] = "Invalid booking details.";
                return RedirectToAction("Index");
            }

            return View(bookingPreview);
        }


        // POST: Book Court (Instant Booking from the Confirm Button)
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int courtId, string selectedTimeSlot, DateTime bookingDate)
        {
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Please log in to book a court.";
                return RedirectToAction("Login", "User");
            }

            var booking = await _bookingService.ConfirmBookingAsync(courtId, selectedTimeSlot, bookingDate, username);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "This time slot is already booked.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Court booked successfully!";
            return RedirectToAction("BookingSummary", new { bookingId = booking.BookingId });
        }

        // DELETE: Cancel Booking
        public async Task<IActionResult> CancelBooking(int id)
        {
            bool isSuccess = await _bookingService.CancelBookingAsync(id);

            if (!isSuccess)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Booking has been successfully deleted.";
            return RedirectToAction("MyBookings");
        }

        // GET: Display Booking Summary
        public async Task<IActionResult> BookingSummary(int bookingId)
        {
            var booking = await _bookingService.GetBookingSummaryAsync(bookingId);

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

            var bookings = await _bookingService.GetMyBookingsAsync(username);

            return View(bookings);
        }

        /*
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
        */
    }
}
