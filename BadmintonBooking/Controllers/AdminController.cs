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

        public async Task<IActionResult> PlayerAccounts()
        {
            var players = await _context.Players.ToListAsync();
            return View(players); 
        }

        public IActionResult CreatePlayer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlayer(Player player)
        {
            _context.Add(player);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PlayerAccounts)); 
        }


        public async Task<IActionResult> EditPlayer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return View(player);
        }

        [HttpPost]
        public async Task<IActionResult> EditPlayer(int id, Player player)
        {
            if (id != player.UserId)
            {
                return NotFound();
            }

            try
            {
                _context.Update(player);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(player.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(PlayerAccounts));
        }

        public async Task<IActionResult> DeletePlayer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        [HttpPost, ActionName("DeletePlayer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PlayerAccounts));
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.UserId == id);
        }
    }
}
