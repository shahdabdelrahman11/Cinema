using Microsoft.AspNetCore.Mvc;
using Cinema.Models;
using Cinema.Services;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IMovieService _movieService;

        public BookingsController(IBookingService bookingService, IMovieService movieService)
        {
            _bookingService = bookingService;
            _movieService = movieService;
        }

        [HttpGet]
        public IActionResult Book(int id)
        {
            var movie = _movieService.GetById(id);

            if (movie == null)
                return NotFound();

            if (!movie.Status)
            {
                TempData["Error"] = "This movie is not available for booking.";
                return RedirectToAction("Index", "Movies");
            }

            var booking = new Booking
            {
                MovieId = movie.Id,
                NumberOfTickets = 1,
                ShowDateTime = movie.DateTime,
                PaymentMethod = "Pay at Cinema"
            };

            ViewBag.MovieName = movie.Name;
            ViewBag.Price = movie.Price;

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(Booking booking)
        {
            ModelState.Remove(nameof(Booking.Movie));
            ModelState.Remove(nameof(Booking.PaymentMethod));

            var movie = _movieService.GetById(booking.MovieId);

            if (movie == null || !movie.Status)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = _bookingService.Create(booking);

                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                foreach (var (key, message) in result.Errors)
                    ModelState.AddModelError(key, message);
            }

            ViewBag.MovieName = movie.Name;
            ViewBag.Price = movie.Price;

            return View(booking);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_bookingService.GetAll());
        }
    }
}