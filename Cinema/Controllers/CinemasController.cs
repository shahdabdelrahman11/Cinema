
using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment environment;

        public CinemasController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            environment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var cinemas = db.Cinemas
                .OrderBy(c => c.Name)
                .ToList();

            return View(cinemas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            global::Cinema.Models.Cinema cinema,
            IFormFile? LogoFile)
        {
            if (!ModelState.IsValid)
                return View(cinema);

            if (LogoFile != null && LogoFile.Length > 0)
            {
                if (!IsValidImage(LogoFile))
                {
                    ModelState.AddModelError(
                        "LogoFile",
                        "Choose a JPG, JPEG, PNG, or WEBP image up to 5 MB.");

                    return View(cinema);
                }

                cinema.Logo = await SaveLogo(LogoFile);
            }

            db.Cinemas.Add(cinema);
            await db.SaveChangesAsync();

            TempData["Success"] = "Cinema added successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema = db.Cinemas.Find(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            global::Cinema.Models.Cinema cinema,
            IFormFile? LogoFile)
        {
            if (id != cinema.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(cinema);

            var existingCinema = db.Cinemas.Find(id);

            if (existingCinema == null)
                return NotFound();

            existingCinema.Name = cinema.Name;
            existingCinema.Description = cinema.Description;
            existingCinema.Address = cinema.Address;

            if (LogoFile != null && LogoFile.Length > 0)
            {
                if (!IsValidImage(LogoFile))
                {
                    ModelState.AddModelError(
                        "LogoFile",
                        "Choose a JPG, JPEG, PNG, or WEBP image up to 5 MB.");

                    return View(cinema);
                }

                string? oldLogo = existingCinema.Logo;
                existingCinema.Logo = await SaveLogo(LogoFile);

                DeleteLogoFile(oldLogo);
            }

            await db.SaveChangesAsync();

            TempData["Success"] = "Cinema updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var cinema = db.Cinemas.Find(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = db.Cinemas.Find(id);

            if (cinema == null)
                return NotFound();

            bool hasMovies = db.Movies.Any(m => m.CinemaId == id);

            if (hasMovies)
            {
                TempData["Error"] =
                    "This cinema cannot be deleted because movies are linked to it.";

                return RedirectToAction(nameof(Index));
            }

            string? logo = cinema.Logo;

            db.Cinemas.Remove(cinema);
            await db.SaveChangesAsync();

            DeleteLogoFile(logo);

            TempData["Success"] = "Cinema deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidImage(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string[] allowedExtensions =
                { ".jpg", ".jpeg", ".png", ".webp" };

            return file.Length <= 5 * 1024 * 1024
                && allowedExtensions.Contains(extension);
        }

        private async Task<string> SaveLogo(IFormFile file)
        {
            string folder = Path.Combine(
                environment.WebRootPath, "images", "cinemas");

            Directory.CreateDirectory(folder);

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/cinemas/{fileName}";
        }

        private void DeleteLogoFile(string? logo)
        {
            if (string.IsNullOrWhiteSpace(logo))
                return;

            string fileName = Path.GetFileName(logo);

            string folder = Path.GetFullPath(
                Path.Combine(environment.WebRootPath, "images", "cinemas"));

            string filePath = Path.GetFullPath(
                Path.Combine(folder, fileName));

            if (filePath.StartsWith(
                    folder + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase)
                && System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
