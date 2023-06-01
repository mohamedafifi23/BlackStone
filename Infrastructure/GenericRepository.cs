using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        #region Add
        public virtual void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
        #endregion

        #region Get
        public virtual IReadOnlyList<TEntity> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = null)
        {
            var query = _dbSet.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (string property in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
      Expression<Func<TEntity, bool>> filter = null,
      Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
      string includeProperties = null)
        {
            var query = _dbSet.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if(includeProperties != null)
            {
                foreach (string property in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }            

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public virtual IReadOnlyList<TEntity> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual TEntity GetById(object id)
        {
            return _dbSet.Find(id);
        }        

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual TEntity GetById(params object[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Keys cannot be null or empty");
            }

            if (keys.Length == 1)
            {
                return _dbSet.Find(keys[0]);
            }

            if (keys.Length == 2)
            {
                return _dbSet.Find(keys[0], keys[1]);
            }

            if (keys.Length == 3)
            {
                return _dbSet.Find(keys[0], keys[1], keys[2]);
            }

            throw new ArgumentException("Too many keys");
        }

        public virtual async Task<TEntity> GetByIdAsync(params object[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Keys cannot be null or empty");
            }

            if (keys.Length == 1)
            {
                return await _dbSet.FindAsync(keys[0]);
            }

            if (keys.Length == 2)
            {
                return await _dbSet.FindAsync(keys[0], keys[1]);
            }

            if (keys.Length == 3)
            {
                return await _dbSet.FindAsync(keys[0], keys[1], keys[2]);
            }

            throw new ArgumentException("Too many keys");
        }
        #endregion

        #region Delete
        public virtual void Delete(object id)
        {
            TEntity entityToDelete = _dbSet.Find(id);

            if (entityToDelete != null)
            {
                Delete(entityToDelete);
            }
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }

            _dbSet.Remove(entityToDelete);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entitiesToDelete)
        {
            foreach (var entityToDelete in entitiesToDelete)
            {
                if (_context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToDelete);
                }
            }

            _dbSet.RemoveRange(entitiesToDelete);
        }
        #endregion

        #region Update
        public virtual void Update(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entitiesToUpdate)
        {
            foreach (var entityToUpdate in entitiesToUpdate)
            {
                if (_context.Entry(entityToUpdate).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToUpdate);
                }
            }

            _dbSet.UpdateRange(entitiesToUpdate);
        }
        #endregion       
    }
}
