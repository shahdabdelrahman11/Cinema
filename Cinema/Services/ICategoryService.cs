using Cinema.Models;
using System.Collections.Generic;

namespace Cinema.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAll();
        Category? GetById(int id);
        void Create(Category category);
        bool Update(Category category);
        bool Delete(int id);
    }
}
