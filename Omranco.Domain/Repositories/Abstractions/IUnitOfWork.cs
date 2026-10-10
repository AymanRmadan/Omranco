using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;

namespace Omranco.Domain.Repositories.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepository<AuditLog, int> AuditLogs { get; }

        IGenaricRepository<Rfq, int> Rfqs { get; }

        IGenaricRepository<Category, int> Categories { get; }

        IGenaricRepository<Vendor, int> Vendors { get; }

        IGenaricRepository<CustomerAccountUser, int> CustomerAccountUsers { get; }

        IGenaricRepository<ProjectMember, int> ProjectMembers { get; }

        IGenaricRepository<RfqInvitation, int> RfqInvitations { get; }
        IGenaricRepository<Bid, int> Bids { get; }

        Task<int> SaveChangeAsync();
    }
}