using System.Collections.Generic;

namespace Cinema.Services
{
    public interface ICinemaService
    {
        IEnumerable<Models.Cinema> GetAll();
        Models.Cinema? GetById(int id);
        ServiceResult Create(Models.Cinema cinema, IFormFile? logoFile);
        ServiceResult Update(Models.Cinema cinema, IFormFile? logoFile);
        ServiceResult Delete(int id);
    }
}
