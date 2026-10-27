
using global::Cinema.Models;

namespace Cinema.ViewModels
{
    public class DashboardVM
    {
        public int MoviesCount { get; set; }
        public int AvailableMovies { get; set; }
        public int CategoriesCount { get; set; }
        public int CinemasCount { get; set; }

        public int BookingsCount { get; set; }
        public decimal TotalBookingValue { get; set; }

        public List<Movie> LatestMovies { get; set; } = new();

        public List<Booking> LatestBookings { get; set; } = new();

        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();

        public List<string> MonthLabels { get; set; } = new();
        public List<int> MonthCounts { get; set; } = new();
    }
}
