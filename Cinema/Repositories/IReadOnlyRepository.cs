using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cinema.Repositories
{
    public interface IReadOnlyRepository<T> where T : class
    {
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes);
        IQueryable<T> Query(params Expression<Func<T, object>>[] includes);
        T? GetById(int id);
        bool Any(Expression<Func<T, bool>> predicate);
    }
}