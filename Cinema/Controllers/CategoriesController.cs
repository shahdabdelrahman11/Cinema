
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoriesController(ApplicationDbContext context)
        {
            db = context;
        }

        
        public IActionResult Index()
        {
            var categories = db.Categories
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            db.Categories.Add(category);
            db.SaveChanges();

            TempData["Success"] = "Category added successfully.";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category category)
        {
            if (id != category.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(category);

            var existingCategory = db.Categories.Find(id);

            if (existingCategory == null)
                return NotFound();

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;

            db.SaveChanges();

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
                return NotFound();

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
