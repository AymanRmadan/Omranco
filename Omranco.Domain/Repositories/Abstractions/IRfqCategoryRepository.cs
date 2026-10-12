namespace Omranco.Domain.Repositories.Abstractions;

public interface IRfqCategoryRepository
{
    Task ReplaceAsync(
        int rfqId,
        IEnumerable<int> categoryIds);
}