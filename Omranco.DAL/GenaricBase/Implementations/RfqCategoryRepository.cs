using Omranco.DAL.Database;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.DAL.Repositories.Implementations;

public sealed class RfqCategoryRepository
    : IRfqCategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RfqCategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task ReplaceAsync(int rfqId, IEnumerable<int> categoryIds)
    {
        var table =
            _dbContext.Set<RfqCategory>();


        var existing =
            await table
                .Where(
                    x => x.RfqId == rfqId
                )
                .ToListAsync();


        if (existing.Count > 0)
        {
            table.RemoveRange(existing);
        }


        var categories =
            categoryIds
                .Distinct()
                .Select(
                    categoryId =>
                        new RfqCategory
                        {
                            RfqId =
                                rfqId,

                            CategoryId =
                                categoryId
                        }
                )
                .ToList();


        if (categories.Count > 0)
        {
            await table.AddRangeAsync(
                categories
            );
        }
    }
}