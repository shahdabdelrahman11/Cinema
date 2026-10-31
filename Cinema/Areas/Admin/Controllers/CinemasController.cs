using Cinema.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemasController : Controller
    {
        private readonly ICinemaService _cinemaService;

        public CinemasController(ICinemaService cinemaService)
        {
            _cinemaService = cinemaService;
        }

        public IActionResult Index()
        {
            return View(_cinemaService.GetAll());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.Cinema cinema, IFormFile? LogoFile)
        {
            if (!ModelState.IsValid)
                return View(cinema);

            var result = _cinemaService.Create(cinema, LogoFile);

            if (!result.Succeeded)
            {
                ModelState.AddServiceErrors(result);
                return View(cinema);
            }

            TempData["Success"] = "Cinema added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema = _cinemaService.GetById(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Models.Cinema cinema, IFormFile? LogoFile)
        {
            if (id != cinema.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(cinema);

            if (_cinemaService.GetById(id) == null)
                return NotFound();

            var result = _cinemaService.Update(cinema, LogoFile);

            if (!result.Succeeded)
            {
                ModelState.AddServiceErrors(result);
                return View(cinema);
            }

            TempData["Success"] = "Cinema updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var cinema = _cinemaService.GetById(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_cinemaService.GetById(id) == null)
                return NotFound();

            var result = _cinemaService.Delete(id);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Errors[0].Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Cinema deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}