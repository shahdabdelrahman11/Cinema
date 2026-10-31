using Cinema.Models;
using System.Collections.Generic;

namespace Cinema.Services
{
    public interface IMovieService
    {
        Movie? GetById(int id);
        Movie? GetByIdWithDetails(int id);
        PagedResult<Movie> GetPaged(string? searchString, int page, int pageSize);

        ServiceResult Create(
            Movie movie,
            IFormFile? imageFile,
            List<IFormFile>? subImageFiles,
            List<int>? actorIds);

        ServiceResult Update(
            Movie movie,
            IFormFile? imageFile,
            List<IFormFile>? subImageFiles,
            List<int>? deleteImageIds,
            List<int>? actorIds);

        bool Delete(int id);
    }
}