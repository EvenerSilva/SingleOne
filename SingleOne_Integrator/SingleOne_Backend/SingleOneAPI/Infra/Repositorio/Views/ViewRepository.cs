using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SingleOneAPI.Infra.Repositorio.Views
{
    public class ViewRepository<T> : IReadOnlyRepository<T> where T : class
    {
        protected readonly SingleOneDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public ViewRepository(SingleOneDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> Buscar(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public IQueryable<T> IncludeWithThenInclude(params Expression<Func<IQueryable<T>, IQueryable<T>>>[] includeExpressions)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeExpression in includeExpressions)
            {
                query = includeExpression.Compile().Invoke(query);
            }

            return query;
        }

        public T ObterPorId(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public IEnumerable<T> ObterTodos()
        {
            return _context.Set<T>().ToList();
        }

        public IQueryable<T> Query()
        {
            return _dbSet;
        }
    }
}
