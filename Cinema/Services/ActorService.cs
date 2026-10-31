using Cinema.Models;
using Cinema.Repositories;
using System.Collections.Generic;

namespace Cinema.Services
{
    public class ActorService : IActorService
    {
        private const string ImageFolder = "actors";
        private const string ImageError = "Choose a JPG, PNG, or WEBP image up to 5 MB.";

        private readonly IRepository<Actor> _actors;
        private readonly IRepository<Movie> _movies;
        private readonly IImageService _images;

        public ActorService(
            IRepository<Actor> actors,
            IRepository<Movie> movies,
            IImageService images)
        {
            _actors = actors;
            _movies = movies;
            _images = images;
        }

        public IEnumerable<Actor> GetAll() =>
            _actors.GetAll().OrderBy(a => a.FirstName).ThenBy(a => a.LastName).ToList();

        public Actor? GetById(int id) => _actors.GetById(id);

        public ServiceResult Create(Actor actor, IFormFile? imgFile)
        {
            var result = new ServiceResult();

            if (imgFile != null && imgFile.Length > 0)
            {
                if (!_images.IsValid(imgFile))
                {
                    result.AddError("ImgFile", ImageError);
                    return result;
                }

                actor.Img = _images.Save(imgFile, ImageFolder);
            }

            _actors.Add(actor);
            _actors.Save();
            return result;
        }

        public ServiceResult Update(Actor actor, IFormFile? imgFile)
        {
            var result = new ServiceResult();

            var existing = _actors.GetById(actor.Id);
            if (existing == null)
            {
                result.AddError(string.Empty, "Actor not found.");
                return result;
            }

            if (imgFile != null && imgFile.Length > 0)
            {
                if (!_images.IsValid(imgFile))
                {
                    result.AddError("ImgFile", ImageError);
                    return result;
                }

                _images.Delete(existing.Img, ImageFolder);
                existing.Img = _images.Save(imgFile, ImageFolder);
            }

            existing.FirstName = actor.FirstName;
            existing.LastName = actor.LastName;
            existing.Bio = actor.Bio;

            _actors.Save();
            return result;
        }

        public ServiceResult Delete(int id)
        {
            var result = new ServiceResult();

            var actor = _actors.GetById(id);
            if (actor == null)
            {
                result.AddError(string.Empty, "Actor not found.");
                return result;
            }

            if (_movies.Any(m => m.Actors.Any(a => a.Id == id)))
            {
                result.AddError(string.Empty,
                    "This actor cannot be deleted because they are linked to movies.");
                return result;
            }

            _images.Delete(actor.Img, ImageFolder);

            _actors.Delete(id);
            _actors.Save();
            return result;
        }
    }
}
