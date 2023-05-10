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
        void Update(TEntity entity);    
        void Delete(object id);    
        TEntity GetById(object id);
        IReadOnlyList<TEntity> GetAll();
        IReadOnlyList<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null
            );
    }
}
