
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;

namespace Cinema.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment env;

        public MoviesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            db = context;
            env = environment;
        }

      
public IActionResult Index(string? searchString, int page = 1)
        {
            int pageSize = 6;

            var query = db.Movies
                .Include(m => m.SubImages)
                .Include(m => m.Actors)
                .AsQueryable();

          
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(m =>
                    m.Name.Contains(searchString.Trim()));
            }

            
            int totalMovies = query.Count();

            int totalPages = (int)Math.Ceiling(
                totalMovies / (double)pageSize);

            page = totalPages == 0
                ? 1
                : Math.Clamp(page, 1, totalPages);

         
            var movies = query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalMovies = totalMovies;

            return View(movies);
        }


     
        [HttpGet]
        public IActionResult Create()
        {
            LoadLists();
            LoadActorList();

            return View(new Movie
            {
                DateTime = DateTime.Now
            });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            Movie movie,
            IFormFile? ImageFile,
            List<IFormFile>? SubImageFiles,
            List<int>? ActorIds)
        {
            ModelState.Remove(nameof(Movie.Category));
            ModelState.Remove(nameof(Movie.Cinema));
            ModelState.Remove(nameof(Movie.Actors));

            ValidateSelections(movie);

            movie.Actors = ActorIds == null
                ? new List<Actor>()
                : db.Actors
                    .Where(a => ActorIds.Contains(a.Id))
                    .ToList();

            if (!ModelState.IsValid)
            {
                LoadLists(movie.CategoryId, movie.CinemaId);
                LoadActorList(ActorIds);
                return View(movie);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                movie.MainImg = SaveFile(ImageFile);
            }

            db.Movies.Add(movie);
            db.SaveChanges();

            if (SubImageFiles != null)
            {
                foreach (var file in SubImageFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        db.MovieSubImages.Add(new MovieSubImage
                        {
                            Img = SaveFile(file),
                            MovieId = movie.Id
                        });
                    }
                }

                db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

       
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = db.Movies
                .Include(m => m.SubImages)
                .Include(m => m.Actors)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            LoadLists(movie.CategoryId, movie.CinemaId);
            LoadActorList(movie.Actors.Select(a => a.Id).ToList());

            return View(movie);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            Movie movie,
            IFormFile? ImageFile,
            List<IFormFile>? SubImageFiles,
            List<int>? DeleteImageIds,
            List<int>? ActorIds)
        {
            if (id != movie.Id)
                return NotFound();

            ModelState.Remove(nameof(Movie.Category));
            ModelState.Remove(nameof(Movie.Cinema));
            ModelState.Remove(nameof(Movie.Actors));

            var existing = db.Movies
                .Include(m => m.SubImages)
                .Include(m => m.Actors)
                .FirstOrDefault(m => m.Id == id);

            if (existing == null)
                return NotFound();

            ValidateSelections(movie);

            var selectedActors = ActorIds == null
                ? new List<Actor>()
                : db.Actors
                    .Where(a => ActorIds.Contains(a.Id))
                    .ToList();

           
            if (ActorIds != null &&
                selectedActors.Count != ActorIds.Distinct().Count())
            {
                ModelState.AddModelError(
                    "ActorIds",
                    "One or more selected actors are invalid.");
            }

            if (!ModelState.IsValid)
            {
                movie.SubImages = existing.SubImages;
                LoadLists(movie.CategoryId, movie.CinemaId);
                LoadActorList(ActorIds);
                return View(movie);
            }

            existing.Name = movie.Name;
            existing.Des = movie.Des;
            existing.Price = movie.Price;
            existing.DateTime = movie.DateTime;
            existing.Status = movie.Status;
            existing.CategoryId = movie.CategoryId;
            existing.CinemaId = movie.CinemaId;

            
            existing.Actors.Clear();

            foreach (var actor in selectedActors)
            {
                existing.Actors.Add(actor);
            }

           
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string? oldImage = existing.MainImg;
                existing.MainImg = SaveFile(ImageFile);
                DeleteFile(oldImage);
            }

            if (DeleteImageIds != null)
            {
                var toDelete = existing.SubImages
                    .Where(s => DeleteImageIds.Contains(s.Id))
                    .ToList();

                foreach (var img in toDelete)
                {
                    DeleteFile(img.Img);
                    db.MovieSubImages.Remove(img);
                }
            }

            if (SubImageFiles != null)
            {
                foreach (var file in SubImageFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        db.MovieSubImages.Add(new MovieSubImage
                        {
                            Img = SaveFile(file),
                            MovieId = existing.Id
                        });
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var movie = db.Movies
                .Include(m => m.SubImages)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            DeleteFile(movie.MainImg);

            foreach (var img in movie.SubImages.ToList())
            {
                DeleteFile(img.Img);
            }

            db.Movies.Remove(movie);
            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

       
        private void ValidateSelections(Movie movie)
        {
            if (movie.CategoryId <= 0 ||
                !db.Categories.Any(c => c.Id == movie.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(movie.CategoryId),
                    "Please select a valid category.");
            }

            if (movie.CinemaId <= 0 ||
                !db.Cinemas.Any(c => c.Id == movie.CinemaId))
            {
                ModelState.AddModelError(
                    nameof(movie.CinemaId),
                    "Please select a valid cinema.");
            }
        }

        private void LoadLists(
            int? categoryId = null,
            int? cinemaId = null)
        {
            ViewBag.Categories = new SelectList(
                db.Categories.ToList(),
                "Id",
                "Name",
                categoryId);

            ViewBag.Cinemas = new SelectList(
                db.Cinemas.ToList(),
                "Id",
                "Name",
                cinemaId);
        }

       
        private void LoadActorList(
            List<int>? selectedActorIds = null)
        {
            var actors = db.Actors
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .Select(a => new
                {
                    a.Id,
                    FullName = a.FirstName + " " + a.LastName
                })
                .ToList();

            ViewBag.Actors = new MultiSelectList(
                actors,
                "Id",
                "FullName",
                selectedActorIds);
        }

        
        private string SaveFile(IFormFile file)
        {
            var webRoot = env.WebRootPath
                ?? Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");

            var folder = Path.Combine(webRoot, "images");

            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString()
                + Path.GetExtension(file.FileName);

            var path = Path.Combine(folder, fileName);

            using var stream = System.IO.File.Create(path);
            file.CopyTo(stream);

            return fileName;
        }

      
        private void DeleteFile(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var webRoot = env.WebRootPath
                ?? Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");

            var path = Path.Combine(webRoot, "images", fileName);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
