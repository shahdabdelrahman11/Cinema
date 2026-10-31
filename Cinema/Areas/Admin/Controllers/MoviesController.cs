using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ICategoryService _categoryService;
        private readonly ICinemaService _cinemaService;
        private readonly IActorService _actorService;

        public MoviesController(
            IMovieService movieService,
            ICategoryService categoryService,
            ICinemaService cinemaService,
            IActorService actorService)
        {
            _movieService = movieService;
            _categoryService = categoryService;
            _cinemaService = cinemaService;
            _actorService = actorService;
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

            if (ModelState.IsValid)
            {
                var result = _movieService.Create(movie, ImageFile, SubImageFiles, ActorIds);

                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                ModelState.AddServiceErrors(result);
            }

            LoadLists(movie.CategoryId, movie.CinemaId);
            LoadActorList(ActorIds);
            return View(movie);
        }

       
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _movieService.GetByIdWithDetails(id);

            if (movie == null)
                return NotFound();

            LoadLists(movie.CategoryId, movie.CinemaId);

            var selectedActorIds = movie.Actors.Select(a => a.Id).ToList();
            LoadActorList(selectedActorIds);

            return View("Edit", movie);
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

            var existing = _movieService.GetByIdWithDetails(id);

            if (existing == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = _movieService.Update(
                    movie, ImageFile, SubImageFiles, DeleteImageIds, ActorIds);

                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                ModelState.AddServiceErrors(result);
            }

            movie.SubImages = existing.SubImages;

            LoadLists(movie.CategoryId, movie.CinemaId);
            LoadActorList(ActorIds);

            return View("Edit", movie);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!_movieService.Delete(id))
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        private void LoadLists(int? categoryId = null, int? cinemaId = null)
        {
            ViewBag.Categories = new SelectList(
                _categoryService.GetAll(), "Id", "Name", categoryId);

            ViewBag.Cinemas = new SelectList(
                _cinemaService.GetAll(), "Id", "Name", cinemaId);
        }

        private void LoadActorList(List<int>? selectedActorIds = null)
        {
            var actors = _actorService.GetAll()
                .Select(a => new
                {
                    a.Id,
                    FullName = a.FirstName + " " + a.LastName
                })
                .ToList();

            ViewBag.Actors = new MultiSelectList(
                actors, "Id", "FullName", selectedActorIds);
        }
    }
}