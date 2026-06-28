

using Main_Structure.Domain.Entities;

namespace Main_Structure.Domain.Repositories.Abstractions
{
    public interface IUnitOfWork
    {
        IGenaricRepository<AuditLog, int> AuditLogs { get; }


        Task<int> SaveChangeAsync();
    }
}
