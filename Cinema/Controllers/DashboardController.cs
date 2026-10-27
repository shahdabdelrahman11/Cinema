
using Cinema.Data;
using global::Cinema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext db;

        public DashboardController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var movies = db.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .ToList();

            var perCategory = movies
                .GroupBy(m => m.Category?.Name ?? "Unknown")
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var perMonth = movies
                .GroupBy(m => new
                {
                    m.DateTime.Year,
                    m.DateTime.Month
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:00}",
                    Count = g.Count()
                })
                .ToList();

            var latestBookings = db.Bookings
                .Include(b => b.Movie)
                .OrderByDescending(b => b.BookingDate)
                .Take(10)
                .ToList();

            var bookingsCount = db.Bookings.Count();

            var totalBookingValue = db.Bookings
                .Where(b => b.Movie != null)
                .Sum(b => (decimal?)(
                    b.Movie!.Price * b.NumberOfTickets
                )) ?? 0m;

            var vm = new DashboardVM
            {
                MoviesCount = movies.Count,
                AvailableMovies = movies.Count(m => m.Status),
                CategoriesCount = db.Categories.Count(),
                CinemasCount = db.Cinemas.Count(),

                BookingsCount = bookingsCount,
                TotalBookingValue = totalBookingValue,
                LatestBookings = latestBookings,

                LatestMovies = movies
                    .OrderByDescending(m => m.Id)
                    .Take(10)
                    .ToList(),

                CategoryLabels = perCategory
                    .Select(x => x.Name)
                    .ToList(),

                CategoryCounts = perCategory
                    .Select(x => x.Count)
                    .ToList(),

                MonthLabels = perMonth
                    .Select(x => x.Label)
                    .ToList(),

                MonthCounts = perMonth
                    .Select(x => x.Count)
                    .ToList()
            };

            return View(vm);
        }
    }
}
