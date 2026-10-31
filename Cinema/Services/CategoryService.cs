using Cinema.Models;
using Cinema.Repositories;
using System.Collections.Generic;

namespace Cinema.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categories;

        public CategoryService(IRepository<Category> categories)
        {
            _categories = categories;
        }

        public IEnumerable<Category> GetAll() =>
            _categories.GetAll().OrderBy(c => c.Name).ToList();

        public Category? GetById(int id) => _categories.GetById(id);

        public void Create(Category category)
        {
            _categories.Add(category);
            _categories.Save();
        }

        public bool Update(Category category)
        {
            var existing = _categories.GetById(category.Id);
            if (existing == null)
                return false;

            existing.Name = category.Name;
            existing.Description = category.Description;

            _categories.Save();
            return true;
        }

        public bool Delete(int id)
        {
            if (_categories.GetById(id) == null)
                return false;

            _categories.Delete(id);
            _categories.Save();
            return true;
        }
    }
}