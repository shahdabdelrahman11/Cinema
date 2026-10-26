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

        public MoviesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            db = context;
            env = environment;
        }

        public IActionResult Index()
        {
            var movies = db.Movies.Include(m => m.SubImages).ToList();
            return View(movies);
        }

        // ---------- CREATE ----------
        public IActionResult Create()
        {
            LoadLists();
            return View(new Movie { DateTime = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movie movie, IFormFile? ImageFile, List<IFormFile> SubImageFiles)
        {
            ModelState.Remove(nameof(Movie.Category));
            ModelState.Remove(nameof(Movie.Cinema));

            if (movie.CategoryId <= 0)
                ModelState.AddModelError("CategoryId", "Please select a category.");
            if (movie.CinemaId <= 0)
                ModelState.AddModelError("CinemaId", "Please select a cinema.");

            if (!ModelState.IsValid)
            {
                LoadLists(movie.CategoryId, movie.CinemaId);
                return View(movie);
            }

            if (ImageFile != null && ImageFile.Length > 0)
                movie.MainImg = SaveFile(ImageFile);

            db.Movies.Add(movie);
            db.SaveChanges();

            foreach (var file in SubImageFiles)
            {
                if (file != null && file.Length > 0)
                {
                    db.Add(new MovieSubImage { Img = SaveFile(file), MovieId = movie.Id });
                }
            }
            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ---------- EDIT ----------
        public IActionResult Edit(int id)
        {
            var movie = db.Movies.Include(m => m.SubImages).FirstOrDefault(m => m.Id == id);
            if (movie == null) return NotFound();

            LoadLists(movie.CategoryId, movie.CinemaId);
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Movie movie, IFormFile? ImageFile,
                                  List<IFormFile> SubImageFiles, List<int> DeleteImageIds)
        {
            if (id != movie.Id) return NotFound();

            ModelState.Remove(nameof(Movie.Category));
            ModelState.Remove(nameof(Movie.Cinema));

            if (movie.CategoryId <= 0)
                ModelState.AddModelError("CategoryId", "Please select a category.");
            if (movie.CinemaId <= 0)
                ModelState.AddModelError("CinemaId", "Please select a cinema.");

            var existing = db.Movies.Include(m => m.SubImages).FirstOrDefault(m => m.Id == id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                movie.SubImages = existing.SubImages;
                LoadLists(movie.CategoryId, movie.CinemaId);
                return View(movie);
            }

            existing.Name = movie.Name;
            existing.Des = movie.Des;
            existing.Price = movie.Price;
            existing.DateTime = movie.DateTime;
            existing.Status = movie.Status;
            existing.CategoryId = movie.CategoryId;
            existing.CinemaId = movie.CinemaId;

            // الصورة الرئيسية
            if (ImageFile != null && ImageFile.Length > 0)
            {
                DeleteFile(existing.MainImg);
                existing.MainImg = SaveFile(ImageFile);
            }

            // حذف الصور الفرعية المحددة
            if (DeleteImageIds != null)
            {
                var toDelete = existing.SubImages.Where(s => DeleteImageIds.Contains(s.Id)).ToList();
                foreach (var img in toDelete)
                {
                    DeleteFile(img.Img);
                    db.Remove(img);
                }
            }

            // إضافة صور فرعية جديدة
            foreach (var file in SubImageFiles)
            {
                if (file != null && file.Length > 0)
                {
                    db.Add(new MovieSubImage { Img = SaveFile(file), MovieId = existing.Id });
                }
            }

            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ---------- DELETE ----------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var movie = db.Movies.Include(m => m.SubImages).FirstOrDefault(m => m.Id == id);
            if (movie == null) return NotFound();

            DeleteFile(movie.MainImg);
            foreach (var img in movie.SubImages)
                DeleteFile(img.Img);

            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private string SaveFile(IFormFile file)
        {
            var folder = Path.Combine(env.WebRootPath, "images");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
            file.CopyTo(stream);
            return fileName;
        }

        private void DeleteFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var path = Path.Combine(env.WebRootPath, "images", fileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private void LoadLists(int? categoryId = null, int? cinemaId = null)
        {
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name", categoryId);
            ViewBag.Cinemas = new SelectList(db.Cinemas.ToList(), "Id", "Name", cinemaId);
        }
    }
}