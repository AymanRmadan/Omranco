using System.Linq.Expressions;

namespace Main_Structure.DAL.GenaricBase.Implementations
{
    public class GenaricRepository<TEntity, Key> : IGenaricRepository<TEntity, Key>
    where TEntity : BaseEntity<Key>
    where Key : struct
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<TEntity> _table;

        public GenaricRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _table = _dbContext.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(Key id)
        {
            return await _table
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id!.Equals(id));
        }

        public async Task<TEntity?> GetByIdAsync(Key id,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _table;

            foreach (var include in includes)
                query = query.Include(include);

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id!.Equals(id));
        }

        public async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> criteria,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _table;

            foreach (var include in includes)
                query = query.Include(include);

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(criteria);
        }

        public async Task<List<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _table;

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes)
                query = query.Include(include);

            if (orderBy != null)
                query = orderBy(query);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? criteria = null)
        {
            return criteria == null
                ? await _table.AnyAsync()
                : await _table.AnyAsync(criteria);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? criteria = null)
        {
            return criteria == null
                ? await _table.CountAsync()
                : await _table.CountAsync(criteria);
        }

        public async Task InsertAsync(TEntity entity)
        {
            await _table.AddAsync(entity);
        }

        public async Task InsertListAsync(IEnumerable<TEntity> entities)
        {
            await _table.AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            _table.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _table.UpdateRange(entities);
        }

        public void Delete(TEntity entity)
        {
            _table.Remove(entity);
        }

        public async Task<TEntity?> FindAsync(params object[] id)
        {
            return await _table.FindAsync(id);
        }

        public IQueryable<TEntity> GetQueryable() => _table.AsNoTracking();

    }
}