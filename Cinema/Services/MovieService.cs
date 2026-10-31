using Cinema.Models;
using Cinema.Repositories;
using System;
using System.Collections.Generic;

namespace Cinema.Services
{
    public class MovieService : IMovieService
    {
        private readonly IRepository<Movie> _movies;
        private readonly IRepository<MovieSubImage> _subImages;
        private readonly IRepository<Actor> _actors;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Models.Cinema> _cinemas;
        private readonly IImageService _images;

        public MovieService(
            IRepository<Movie> movies,
            IRepository<MovieSubImage> subImages,
            IRepository<Actor> actors,
            IRepository<Category> categories,
            IRepository<Models.Cinema> cinemas,
            IImageService images)
        {
            _movies = movies;
            _subImages = subImages;
            _actors = actors;
            _categories = categories;
            _cinemas = cinemas;
            _images = images;
        }

        public Movie? GetById(int id) => _movies.GetById(id);

        public Movie? GetByIdWithDetails(int id) =>
            _movies.Query(m => m.SubImages, m => m.Actors)
                   .FirstOrDefault(m => m.Id == id);

        public PagedResult<Movie> GetPaged(string? searchString, int page, int pageSize)
        {
            var query = _movies.Query(m => m.SubImages, m => m.Actors);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var term = searchString.Trim();
                query = query.Where(m => m.Name.Contains(term));
            }

            int totalMovies = query.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            page = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

            var items = query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Movie>
            {
                Items = items,
                Page = page,
                TotalPages = totalPages,
                TotalCount = totalMovies
            };
        }

        public ServiceResult Create(
            Movie movie,
            IFormFile? imageFile,
            List<IFormFile>? subImageFiles,
            List<int>? actorIds)
        {
            var result = new ServiceResult();

            ValidateSelections(movie, result);
            if (!result.Succeeded)
                return result;

            movie.Actors = GetActors(actorIds);

            if (imageFile != null && imageFile.Length > 0)
                movie.MainImg = _images.SaveFile(imageFile);

            _movies.Add(movie);
            _movies.Save();

            AddSubImages(movie.Id, subImageFiles);
            _movies.Save();

            return result;
        }

        public ServiceResult Update(
            Movie movie,
            IFormFile? imageFile,
            List<IFormFile>? subImageFiles,
            List<int>? deleteImageIds,
            List<int>? actorIds)
        {
            var result = new ServiceResult();

            var existing = GetByIdWithDetails(movie.Id);
            if (existing == null)
            {
                result.AddError(string.Empty, "Movie not found.");
                return result;
            }

            ValidateSelections(movie, result);

            var selectedActors = GetActors(actorIds);

            if (actorIds != null && selectedActors.Count != actorIds.Distinct().Count())
                result.AddError("ActorIds", "One or more selected actors are invalid.");

            if (!result.Succeeded)
                return result;

            existing.Name = movie.Name;
            existing.Des = movie.Des;
            existing.Price = movie.Price;
            existing.DateTime = movie.DateTime;
            existing.Status = movie.Status;
            existing.CategoryId = movie.CategoryId;
            existing.CinemaId = movie.CinemaId;

            existing.Actors.Clear();
            foreach (var actor in selectedActors)
                existing.Actors.Add(actor);

            if (imageFile != null && imageFile.Length > 0)
            {
                var oldImage = existing.MainImg;
                existing.MainImg = _images.SaveFile(imageFile);
                _images.DeleteFile(oldImage);
            }

            if (deleteImageIds != null)
            {
                var toDelete = existing.SubImages
                    .Where(s => deleteImageIds.Contains(s.Id))
                    .ToList();

                foreach (var img in toDelete)
                {
                    _images.DeleteFile(img.Img);
                    _subImages.Delete(img.Id);
                }
            }

            AddSubImages(existing.Id, subImageFiles);

            _movies.Save();
            return result;
        }

        public bool Delete(int id)
        {
            var movie = _movies.Query(m => m.SubImages).FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return false;

            _images.DeleteFile(movie.MainImg);

            foreach (var img in movie.SubImages.ToList())
                _images.DeleteFile(img.Img);

            _movies.Delete(id);
            _movies.Save();
            return true;
        }

        private void ValidateSelections(Movie movie, ServiceResult result)
        {
            if (movie.CategoryId <= 0 || !_categories.Any(c => c.Id == movie.CategoryId))
                result.AddError(nameof(Movie.CategoryId), "Please select a valid category.");

            if (movie.CinemaId <= 0 || !_cinemas.Any(c => c.Id == movie.CinemaId))
                result.AddError(nameof(Movie.CinemaId), "Please select a valid cinema.");
        }

        private List<Actor> GetActors(List<int>? actorIds)
        {
            if (actorIds == null)
                return new List<Actor>();

            return _actors.Query().Where(a => actorIds.Contains(a.Id)).ToList();
        }

        private void AddSubImages(int movieId, List<IFormFile>? files)
        {
            if (files == null)
                return;

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    _subImages.Add(new MovieSubImage
                    {
                        Img = _images.SaveFile(file),
                        MovieId = movieId
                    });
                }
            }
        }
    }
}