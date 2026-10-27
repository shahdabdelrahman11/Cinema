
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;

namespace Cinema.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext db;

        public BookingsController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Book(int id)
        {
            var movie = db.Movies.FirstOrDefault(m => m.Id == id);

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

            var movie = db.Movies.FirstOrDefault(
                m => m.Id == booking.MovieId);

            if (movie == null || !movie.Status)
            {
                return NotFound();
            }

            if (booking.NumberOfTickets < 1 ||
                booking.NumberOfTickets > 10)
            {
                ModelState.AddModelError(
                    nameof(booking.NumberOfTickets),
                    "Choose between 1 and 10 tickets.");
            }

            if (booking.ShowDateTime < DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(booking.ShowDateTime),
                    "Please choose a future show date and time.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MovieName = movie.Name;
                ViewBag.Price = movie.Price;

                return View(booking);
            }

            try
            {
                booking.BookingDate = DateTime.Now;
                booking.PaymentMethod = "Pay at Cinema";

                booking.Id = 0;

                db.Entry(booking).State = EntityState.Added;
                db.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.Message ?? ex.Message;

                ModelState.AddModelError(
                    string.Empty,
                    "Database Error: " + error);

                ViewBag.MovieName = movie.Name;
                ViewBag.Price = movie.Price;

                return View(booking);
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            var bookings = db.Bookings
                .Include(b => b.Movie)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View(bookings);
        }
    }
}
