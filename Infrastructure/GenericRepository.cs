using Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _context;
        private DbSet<TEntity> _dbSet;
        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();    
        }

        public virtual void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public virtual IReadOnlyList<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public virtual IReadOnlyList<TEntity> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual TEntity GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;    
        }
    }
}
