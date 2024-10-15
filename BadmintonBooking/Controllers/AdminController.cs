using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using BadmintonBooking.Data;
using Microsoft.Extensions.Logging; 

namespace BadmintonBooking.Controllers
{
    public class AdminController : Controller
    {
        private readonly BadmintonBookingContext _context;
        private readonly ILogger<AdminController> _logger; 

        public AdminController(BadmintonBookingContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger; 
        }

        // GET: Admin/Bookings
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)
                .ToListAsync();

            // Group by date and count bookings per day
            var bookingsPerDay = bookings
                .GroupBy(b => b.StartTime.Date)
                .Select(g => new {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Date)
                .ToList();

            // Prepare chart data
            var chartData = new
            {
                Labels = bookingsPerDay.Select(b => b.Date.ToString("yyyy-MM-dd")).ToArray(),
                Counts = bookingsPerDay.Select(b => b.Count).ToArray()
            };

            // Use logger to output chart data
            _logger.LogInformation("Chart Data: {@ChartData}", chartData);
            ViewBag.ChartData = chartData;

            return View(bookings);
        }
    }
}
