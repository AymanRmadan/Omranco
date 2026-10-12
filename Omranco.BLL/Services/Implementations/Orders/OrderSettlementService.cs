using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.BLL.Services.Abstractions.WalletServices;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Payments;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.OrderServices;

public sealed class OrderSettlementService
    : IOrderSettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVendorWalletService _vendorWalletService;


    public OrderSettlementService(
        IUnitOfWork unitOfWork,
        IVendorWalletService vendorWalletService)
    {
        _unitOfWork =
            unitOfWork;

        _vendorWalletService =
            vendorWalletService;
    }


    public async Task<Result> TrySettleAsync(
        int orderId)
    {
        var order =
            await _unitOfWork
                .Orders
                .GetByIdAsync(
                    orderId
                );


        if (order is null)
        {
            return Result.Failure(
                OrderErrors.NotFound
            );
        }


        /*
         * Settlement only happens after delivery.
         */
        if (order.Status !=
            OrderStatus.Delivered)
        {
            return Result.Success();
        }


        var payment =
            await _unitOfWork
                .Payments
                .FirstOrDefaultAsync(
                    x =>
                        x.OrderId ==
                        orderId
                );


        if (payment is null)
        {
            return Result.Failure(
                PaymentErrors.NotFound
            );
        }


        /*
         * Partial refund is still eligible.
         * CreditOrderEarningAsync calculates
         * the adjusted vendor earning.
         */
        if (
            payment.Status !=
                PaymentStatus.Paid
            &&
            payment.Status !=
                PaymentStatus.PartiallyRefunded
        )
        {
            return Result.Success();
        }


        var walletResult =
            await _vendorWalletService
                .CreditOrderEarningAsync(
                    orderId
                );


        if (walletResult.IsFailure)
        {
            return Result.Failure(
                walletResult.Error
            );
        }


        return Result.Success();
    }
}