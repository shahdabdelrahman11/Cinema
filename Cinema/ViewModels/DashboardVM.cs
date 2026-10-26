using Cinema.Models;

namespace Cinema.ViewModels
{
    public class DashboardVM
    {
        public int MoviesCount { get; set; }
        public int AvailableMovies { get; set; }
        public int CategoriesCount { get; set; }
        public int CinemasCount { get; set; }
        public int ActorsCount { get; set; }
        public decimal TotalPrice { get; set; }

        public List<Movie> LatestMovies { get; set; } = new();
        public Dictionary<string, int> MoviesPerCategory { get; set; } = new();
    }
}