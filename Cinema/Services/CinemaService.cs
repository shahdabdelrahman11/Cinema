using Cinema.Repositories;
using System.Collections.Generic;

namespace Cinema.Services
{
    public class CinemaService : ICinemaService
    {
        private const string ImageFolder = "cinemas";
        private const string ImageError = "Choose a JPG, JPEG, PNG, or WEBP image up to 5 MB.";

        private readonly IRepository<Models.Cinema> _cinemas;
        private readonly IRepository<Models.Movie> _movies;
        private readonly IImageService _images;

        public CinemaService(
            IRepository<Models.Cinema> cinemas,
            IRepository<Models.Movie> movies,
            IImageService images)
        {
            _cinemas = cinemas;
            _movies = movies;
            _images = images;
        }

        public IEnumerable<Models.Cinema> GetAll() =>
            _cinemas.GetAll().OrderBy(c => c.Name).ToList();

        public Models.Cinema? GetById(int id) => _cinemas.GetById(id);

        public ServiceResult Create(Models.Cinema cinema, IFormFile? logoFile)
        {
            var result = new ServiceResult();

            if (logoFile != null && logoFile.Length > 0)
            {
                if (!_images.IsValid(logoFile))
                {
                    result.AddError("LogoFile", ImageError);
                    return result;
                }

                cinema.Logo = _images.Save(logoFile, ImageFolder);
            }

            _cinemas.Add(cinema);
            _cinemas.Save();
            return result;
        }

        public ServiceResult Update(Models.Cinema cinema, IFormFile? logoFile)
        {
            var result = new ServiceResult();

            var existing = _cinemas.GetById(cinema.Id);
            if (existing == null)
            {
                result.AddError(string.Empty, "Cinema not found.");
                return result;
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                if (!_images.IsValid(logoFile))
                {
                    result.AddError("LogoFile", ImageError);
                    return result;
                }

                var oldLogo = existing.Logo;
                existing.Logo = _images.Save(logoFile, ImageFolder);
                _images.Delete(oldLogo, ImageFolder);
            }

            existing.Name = cinema.Name;
            existing.Description = cinema.Description;
            existing.Address = cinema.Address;

            _cinemas.Save();
            return result;
        }

        public ServiceResult Delete(int id)
        {
            var result = new ServiceResult();

            var cinema = _cinemas.GetById(id);
            if (cinema == null)
            {
                result.AddError(string.Empty, "Cinema not found.");
                return result;
            }

            if (_movies.Any(m => m.CinemaId == id))
            {
                result.AddError(string.Empty,
                    "This cinema cannot be deleted because movies are linked to it.");
                return result;
            }

            var logo = cinema.Logo;

            _cinemas.Delete(id);
            _cinemas.Save();
            _images.Delete(logo, ImageFolder);

            return result;
        }
    }
}