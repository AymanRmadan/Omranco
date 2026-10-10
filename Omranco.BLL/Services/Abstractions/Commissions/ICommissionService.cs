using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Commissions.Responses;

namespace Omranco.BLL.Services.Abstractions.CommissionServices
{
    public interface ICommissionService
    {
        Task<Result<CommissionCalculationResponse>> CalculateAsync(decimal amount);
    }
}