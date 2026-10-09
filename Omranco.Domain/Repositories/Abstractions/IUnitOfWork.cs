

using Omranco.Domain.Entities;

namespace Omranco.Domain.Repositories.Abstractions
{
    public interface IUnitOfWork
    {
        IGenaricRepository<AuditLog, int> AuditLogs { get; }


        Task<int> SaveChangeAsync();
    }
}
