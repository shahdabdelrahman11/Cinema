using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorsController : Controller
    {
        private readonly IActorService _actorService;

        public ActorsController(IActorService actorService)
        {
            _actorService = actorService;
        }

        public IActionResult Index()
        {
            return View(_actorService.GetAll());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Actor actor, IFormFile? ImgFile)
        {
            if (!ModelState.IsValid)
                return View(actor);

            var result = _actorService.Create(actor, ImgFile);

            if (!result.Succeeded)
            {
                ModelState.AddServiceErrors(result);
                return View(actor);
            }

            TempData["Success"] = "Actor added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var actor = _actorService.GetById(id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Actor actor, IFormFile? ImgFile)
        {
            if (id != actor.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(actor);

            if (_actorService.GetById(id) == null)
                return NotFound();

            var result = _actorService.Update(actor, ImgFile);

            if (!result.Succeeded)
            {
                ModelState.AddServiceErrors(result);
                return View(actor);
            }

            TempData["Success"] = "Actor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var actor = _actorService.GetById(id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_actorService.GetById(id) == null)
                return NotFound();

            var result = _actorService.Delete(id);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Errors[0].Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Actor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}