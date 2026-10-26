using Cinema.Data;
using Cinema.ViewModels;
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
            var vm = new DashboardVM
            {
                MoviesCount = db.Movies.Count(),
                AvailableMovies = db.Movies.Count(m => m.Status),
                CategoriesCount = db.Categories.Count(),
                CinemasCount = db.Cinemas.Count(),
                ActorsCount = db.Actors.Count(),
                TotalPrice = db.Movies.Sum(m => (decimal?)m.Price) ?? 0,

                LatestMovies = db.Movies
                    .Include(m => m.Category)
                    .Include(m => m.Cinema)
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .ToList(),

                MoviesPerCategory = db.Movies
                    .Include(m => m.Category)
                    .GroupBy(m => m.Category!.Name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(vm);
        }
    }
}