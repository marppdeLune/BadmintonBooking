using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.Data;
using Microsoft.AspNetCore.Http;

namespace BadmintonBooking.Controllers
{
    public class UserController : Controller
    {
        private readonly BadmintonBookingContext _context;

        public UserController(BadmintonBookingContext context)
        {
            _context = context;
        }

        // GET: Login page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var loginUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == user.Password);

            if (loginUser == null)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return View();
            }

            // Set the user in the session
            HttpContext.Session.SetString("Username", loginUser.Username);
            HttpContext.Session.SetString("Role", loginUser.Role);

            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Booking");
        }

        // GET: Registration page
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return View();
            }

            // Check if the username already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Username already exists.";
                return View();
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful!";
            return RedirectToAction("Login");
        }

        // GET: Logout
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
