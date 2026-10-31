using Cinema.Models;
using Cinema.Repositories;
using Cinema.ViewModels;

namespace Cinema.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Movie> _movies;
        private readonly IRepository<Booking> _bookings;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Models.Cinema> _cinemas;

        public DashboardService(
            IRepository<Movie> movies,
            IRepository<Booking> bookings,
            IRepository<Category> categories,
            IRepository<Models.Cinema> cinemas)
        {
            _movies = movies;
            _bookings = bookings;
            _categories = categories;
            _cinemas = cinemas;
        }

        public DashboardVM GetDashboard()
        {
            var movies = _movies
                .GetAll(m => m.Category, m => m.Cinema)
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

            var latestBookings = _bookings
                .Query(b => b.Movie!)
                .OrderByDescending(b => b.BookingDate)
                .Take(10)
                .ToList();

            var bookingsCount = _bookings.Query().Count();

            var totalBookingValue = _bookings
                .Query()
                .Where(b => b.Movie != null)
                .Sum(b => (decimal?)(
                    b.Movie!.Price * b.NumberOfTickets
                )) ?? 0m;

            return new DashboardVM
            {
                MoviesCount = movies.Count,
                AvailableMovies = movies.Count(m => m.Status),
                CategoriesCount = _categories.Query().Count(),
                CinemasCount = _cinemas.Query().Count(),

                BookingsCount = bookingsCount,
                TotalBookingValue = totalBookingValue,
                LatestBookings = latestBookings,

                LatestMovies = movies
                    .OrderByDescending(m => m.Id)
                    .Take(10)
                    .ToList(),

                CategoryLabels = perCategory.Select(x => x.Name).ToList(),
                CategoryCounts = perCategory.Select(x => x.Count).ToList(),

                MonthLabels = perMonth.Select(x => x.Label).ToList(),
                MonthCounts = perMonth.Select(x => x.Count).ToList()
            };
        }
    }
}