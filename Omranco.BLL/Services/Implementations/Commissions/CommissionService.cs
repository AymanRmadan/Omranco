using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Commissions.Responses;
using Omranco.BLL.Services.Abstractions.CommissionServices;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.CommissionServices
{
    public sealed class CommissionService
        : ICommissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommissionService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<CommissionCalculationResponse>>
            CalculateAsync(decimal amount)
        {
            if (amount <= 0)
            {
                return Result
                    .Failure<CommissionCalculationResponse>(
                        CommissionErrors.InvalidAmount
                    );
            }


            var now = DateTime.UtcNow;


            var rules =
                await _unitOfWork.CommissionRules
                    .GetAllAsync(
                        filter: x =>
                            x.IsActive &&
                            x.ValidFrom <= now &&
                            (
                                !x.ValidTo.HasValue ||
                                x.ValidTo.Value > now
                            ),

                        orderBy: query =>
                            query.OrderByDescending(
                                x => x.ValidFrom
                            ),

                        take: 1
                    );


            var rule = rules.FirstOrDefault();


            if (rule is null)
            {
                return Result
                    .Failure<CommissionCalculationResponse>(
                        CommissionErrors.NoActiveRule
                    );
            }


            if (rule.Percentage is < 0 or > 100)
            {
                return Result
                    .Failure<CommissionCalculationResponse>(
                        CommissionErrors.InvalidPercentage
                    );
            }


            var commissionAmount =
                Math.Round(
                    amount * rule.Percentage / 100m,
                    2
                );


            var vendorNetAmount =
                amount - commissionAmount;


            var response =
                new CommissionCalculationResponse(
                    Rate: rule.Percentage,
                    CommissionAmount: commissionAmount,
                    VendorNetAmount: vendorNetAmount
                );


            return Result.Success(response);
        }
    }
}