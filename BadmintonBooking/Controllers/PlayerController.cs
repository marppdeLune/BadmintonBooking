using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.Data;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Numerics;

namespace BadmintonBooking.Controllers
{
    public class PlayerController : Controller
    {
        private readonly BadmintonBookingContext _context;

        public PlayerController(BadmintonBookingContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ViewPlayerInfo()
        {
            string username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            // Check if the user is a Player
            if (!(user is Player player))
            {
                var registeredPlayer = new Player
                {
                    Username = user.Username,
                    Address = "",
                    Email = null,
                    Phone = null,
                    CreditCard = null,
                    FullName = null
                };

                TempData["InfoMessage"] = "User is not registered as a player. You can add player information below.";
                return View("UpdatePlayerInfo", registeredPlayer); 
            }
            return View("ViewPlayerInfo", player);
        }

        public IActionResult UpdatePlayerInfo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePlayerInfo(Player updatedPlayer)
        {
            string username = HttpContext.Session.GetString("Username");
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return View(updatedPlayer);
            }

            if (existingUser is Player existingPlayer)
            {
                existingPlayer.FullName = !string.IsNullOrWhiteSpace(updatedPlayer.FullName) ? updatedPlayer.FullName : existingPlayer.FullName;
                existingPlayer.Address = !string.IsNullOrWhiteSpace(updatedPlayer.Address) ? updatedPlayer.Address : existingPlayer.Address;
                existingPlayer.Email = !string.IsNullOrWhiteSpace(updatedPlayer.Email) ? updatedPlayer.Email : existingPlayer.Email;
                existingPlayer.Phone = !string.IsNullOrWhiteSpace(updatedPlayer.Phone) ? updatedPlayer.Phone : existingPlayer.Phone;
                existingPlayer.CreditCard = !string.IsNullOrWhiteSpace(updatedPlayer.CreditCard) ? updatedPlayer.CreditCard : existingPlayer.CreditCard;
                _context.Users.Update(existingPlayer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Player info updated successfully!";
                return View("ViewPlayerInfo", existingPlayer);
            }
            else
            {
                var newPlayer = new Player
                {
                    UserId = existingUser.UserId, 
                    Username = existingUser.Username,
                    Password = existingUser.Password,
                    Role = "Player", 

                    FullName = updatedPlayer.FullName,
                    Address = updatedPlayer.Address,
                    Email = updatedPlayer.Email,
                    Phone = updatedPlayer.Phone,
                    CreditCard = updatedPlayer.CreditCard
                };
                _context.Users.Remove(existingUser);
                _context.Users.Add(newPlayer);     
                await _context.SaveChangesAsync(); 

                TempData["SuccessMessage"] = "Player info added successfully!";
                return View("ViewPlayerInfo", newPlayer);
            }
        }

    }
}
