using BadmintonBooking.Models;

namespace BadmintonBooking.Services
{
    public interface IBookingService
    {
        Task<Dictionary<int, List<string>>> GetCourtAvailabilityAsync(DateTime? bookingDate);
        Task<Booking> GetBookingPreviewAsync(int courtId, string selectedTimeSlot, DateTime bookingDate, string username);
        Task<Booking> ConfirmBookingAsync(int courtId, string selectedTimeSlot, DateTime bookingDate, string username);
        Task<bool> CancelBookingAsync(int id);
        Task<Booking> GetBookingSummaryAsync(int bookingId);
        Task<List<Booking>> GetMyBookingsAsync(string username);
    }
}
