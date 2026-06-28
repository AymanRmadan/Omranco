using System.Linq.Expressions;

namespace Main_Structure.Domain.Repositories.Abstractions
{
    public interface IGenaricRepository<TEntity, Key> where Key : struct
    {
        Task<TEntity?> GetByIdAsync(Key id);
        Task<TEntity?> GetByIdAsync(Key id,
            params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> criteria,
            params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, object>>[] includes);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? criteria = null);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? criteria = null);
        Task InsertAsync(TEntity entity);
        Task InsertListAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        Task<TEntity?> FindAsync(params object[] id);

        IQueryable<TEntity> GetQueryable();

    }
}