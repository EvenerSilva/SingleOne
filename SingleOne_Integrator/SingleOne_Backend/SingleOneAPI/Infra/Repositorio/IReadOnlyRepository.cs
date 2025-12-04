using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SingleOneAPI.Infra.Repositorio
{
    public interface IReadOnlyRepository<T> where T : class
    {
        IEnumerable<T> ObterTodos();
        IQueryable<T> Buscar(Expression<Func<T, bool>> predicate);
        T ObterPorId(int id);
        IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> IncludeWithThenInclude(params Expression<Func<IQueryable<T>, IQueryable<T>>>[] includeExpressions);
        IQueryable<T> Query();
    }
}
