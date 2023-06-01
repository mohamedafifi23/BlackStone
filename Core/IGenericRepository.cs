using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);
        Task AddAsync(TEntity entity);   
        void AddRange(IEnumerable<TEntity> entities);   
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        TEntity GetById(object id);
        TEntity GetById(params object[] keys);
        Task<TEntity> GetByIdAsync(object id);
        Task<TEntity> GetByIdAsync(params object[] keys);
        IReadOnlyList<TEntity> GetAll();
        Task<IReadOnlyList<TEntity>> GetAllAsync();
        IReadOnlyList<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null
            );
        Task<IReadOnlyList<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null
            );

        void Update(TEntity entity);    
        void UpdateRange(IEnumerable<TEntity> entities);

        void Delete(object id);
        void Delete(TEntity entityToDelete);
        void DeleteRange(IEnumerable<TEntity> entities);                  
    }
}
