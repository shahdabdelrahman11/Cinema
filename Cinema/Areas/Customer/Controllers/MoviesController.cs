using Cinema.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public IActionResult Index(string? searchString, int page = 1)
        {
            var result = _movieService.GetPaged(searchString, page, 6);

            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalMovies = result.TotalCount;

            return View(result.Items);
        }
    }
}