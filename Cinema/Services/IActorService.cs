using Cinema.Models;
using System.Collections.Generic;

namespace Cinema.Services
{
    public interface IActorService
    {
        IEnumerable<Actor> GetAll();
        Actor? GetById(int id);
        ServiceResult Create(Actor actor, IFormFile? imgFile);
        ServiceResult Update(Actor actor, IFormFile? imgFile);
        ServiceResult Delete(int id);
    }
}
