namespace Main_Structure.DAL.GenaricBase.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private IGenaricRepository<AuditLog, int> _auditLogs;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }


        // ??= => for Lazy loading
        public IGenaricRepository<AuditLog, int> AuditLogs => _auditLogs ??= new GenaricRepository<AuditLog, int>(_dbContext);

        public Task<int> SaveChangeAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
