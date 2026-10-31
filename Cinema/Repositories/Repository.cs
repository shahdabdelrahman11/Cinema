using Cinema.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cinema.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> Query(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
                query = query.Include(include);

            return query;
        }

        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes)
            => Query(includes).ToList();

        public T? GetById(int id) => _dbSet.Find(id);

        public bool Any(Expression<Func<T, bool>> predicate) => _dbSet.Any(predicate);

        public void Add(T entity) => _dbSet.Add(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public void Save() => _context.SaveChanges();
    }
}