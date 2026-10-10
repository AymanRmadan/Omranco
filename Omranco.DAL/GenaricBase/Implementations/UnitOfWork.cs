using Omranco.DAL.Database;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.DAL.GenaricBase.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private IGenaricRepository<AuditLog, int>? _auditLogs;

        private IGenaricRepository<Rfq, int>? _rfqs;
        private IGenaricRepository<Category, int>? _categories;
        private IGenaricRepository<Vendor, int>? _vendors;

        private IGenaricRepository<CustomerAccountUser, int>?
            _customerAccountUsers;

        private IGenaricRepository<ProjectMember, int>?
            _projectMembers;

        private IGenaricRepository<RfqInvitation, int>?
            _rfqInvitations;

        private IGenaricRepository<Bid, int>? _bids;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IGenaricRepository<AuditLog, int> AuditLogs =>
            _auditLogs ??=
                new GenaricRepository<AuditLog, int>(_dbContext);


        public IGenaricRepository<Rfq, int> Rfqs =>
            _rfqs ??=
                new GenaricRepository<Rfq, int>(_dbContext);


        public IGenaricRepository<Category, int> Categories =>
            _categories ??=
                new GenaricRepository<Category, int>(_dbContext);


        public IGenaricRepository<Vendor, int> Vendors =>
            _vendors ??=
                new GenaricRepository<Vendor, int>(_dbContext);


        public IGenaricRepository<CustomerAccountUser, int>
            CustomerAccountUsers =>
                _customerAccountUsers ??=
                    new GenaricRepository<CustomerAccountUser, int>(
                        _dbContext
                    );


        public IGenaricRepository<ProjectMember, int>
            ProjectMembers =>
                _projectMembers ??=
                    new GenaricRepository<ProjectMember, int>(
                        _dbContext
                    );


        public IGenaricRepository<RfqInvitation, int>
            RfqInvitations =>
                _rfqInvitations ??=
                    new GenaricRepository<RfqInvitation, int>(
                        _dbContext
                    );


        public IGenaricRepository<Bid, int> Bids =>
            _bids ??=
                new GenaricRepository<Bid, int>(_dbContext);

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