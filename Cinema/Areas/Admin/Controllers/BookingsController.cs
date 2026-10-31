using Microsoft.AspNetCore.Mvc;
using Cinema.Services;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_bookingService.GetAll());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!_bookingService.Delete(id))
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}