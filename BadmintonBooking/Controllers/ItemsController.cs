using BadmintonBooking.Models;
using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Data;
using Microsoft.EntityFrameworkCore;

namespace BadmintonBooking.Controllers
{
    public class ItemsController : Controller
    {
        private readonly BadmintonBookingContext _context;
		public ItemsController(BadmintonBookingContext context)
		{
			_context = context;
		}

        public async Task<IActionResult> Index()
        {
            var item = await _context.Items.ToListAsync();
            return View(item);
        }
        public IActionResult Create()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name")] Item item)
        {
            if(ModelState.IsValid)
			{
				_context.Items.Add(item);
				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View(item);
		}
	}
}
