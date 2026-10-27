
using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment environment;

        public ActorsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            environment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var actors = await db.Actors
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToListAsync();

            return View(actors);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            global::Cinema.Models.Actor actor,
            IFormFile? ImgFile)
        {
            if (!ModelState.IsValid)
                return View(actor);

            if (ImgFile != null && ImgFile.Length > 0)
            {
                if (!IsValidImage(ImgFile))
                {
                    ModelState.AddModelError(
                        "ImgFile",
                        "Choose a JPG, PNG, or WEBP image up to 5 MB.");

                    return View(actor);
                }

                actor.Img = await SaveImage(ImgFile);
            }

            db.Actors.Add(actor);
            await db.SaveChangesAsync();

            TempData["Success"] = "Actor added successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var actor = await db.Actors.FindAsync(id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            global::Cinema.Models.Actor actor,
            IFormFile? ImgFile)
        {
            if (id != actor.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(actor);

            var existingActor = await db.Actors.FindAsync(id);

            if (existingActor == null)
                return NotFound();

            if (ImgFile != null && ImgFile.Length > 0)
            {
                if (!IsValidImage(ImgFile))
                {
                    ModelState.AddModelError(
                        "ImgFile",
                        "Choose a JPG, PNG, or WEBP image up to 5 MB.");

                    return View(actor);
                }

                DeleteImage(existingActor.Img);
                existingActor.Img = await SaveImage(ImgFile);
            }

            existingActor.FirstName = actor.FirstName;
            existingActor.LastName = actor.LastName;
            existingActor.Bio = actor.Bio;

            await db.SaveChangesAsync();

            TempData["Success"] = "Actor updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await db.Actors.FindAsync(id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await db.Actors.FindAsync(id);

            if (actor == null)
                return NotFound();

            bool hasMovies = await db.Movies
                .AnyAsync(m => m.Actors.Any(a => a.Id == id));

            if (hasMovies)
            {
                TempData["Error"] =
                    "This actor cannot be deleted because they are linked to movies.";

                return RedirectToAction(nameof(Index));
            }

            DeleteImage(actor.Img);

            db.Actors.Remove(actor);
            await db.SaveChangesAsync();

            TempData["Success"] = "Actor deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            string[] allowedExtensions =
                { ".jpg", ".jpeg", ".png", ".webp" };

            return allowedExtensions.Contains(extension)
                   && file.Length <= 5 * 1024 * 1024;
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            string folder = Path.Combine(
                environment.WebRootPath,
                "images",
                "actors");

            Directory.CreateDirectory(folder);

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/actors/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            string fileName = Path.GetFileName(imagePath);

            string folder = Path.Combine(
                environment.WebRootPath,
                "images",
                "actors");

            string filePath = Path.Combine(folder, fileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}
