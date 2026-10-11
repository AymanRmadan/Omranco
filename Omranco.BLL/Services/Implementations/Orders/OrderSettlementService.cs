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
        _unitOfWork = unitOfWork;
        _vendorWalletService = vendorWalletService;
    }


    public async Task<Result> TrySettleAsync(
        int orderId)
    {
        // ==========================================
        // Get Order
        // ==========================================

        var order =
            await _unitOfWork.Orders
                .GetByIdAsync(orderId);


        if (order is null)
        {
            /*
             * استخدم OrderErrors.NotFound
             * الموجود عندك لو بالفعل عاملينه.
             */
            return Result.Failure(
                OrderErrors.NotFound
            );
        }


        // ==========================================
        // Order must be Delivered
        // ==========================================

        if (order.Status !=
            OrderStatus.Delivered)
        {
            /*
             * مش Error.
             *
             * معناها ببساطة إننا لسه
             * مستنيين التسليم.
             */
            return Result.Success();
        }


        // ==========================================
        // Payment
        // ==========================================

        var payment =
            await _unitOfWork.Payments
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId
                );


        if (payment is null)
        {
            /*
             * المفروض كل Order عندنا يكون
             * له Payment، لكن لو لسبب ما
             * مش موجودة فدي مشكلة حقيقية.
             */
            return Result.Failure(
                PaymentErrors.NotFound
            );
        }


        // ==========================================
        // Payment must be Paid
        // ==========================================

        if (payment.Status !=
            PaymentStatus.Paid)
        {
            /*
             * برضه مش Error.
             *
             * الـOrder اتسلمت لكن لسه
             * الدفع غير مؤكد.
             */
            return Result.Success();
        }


        // ==========================================
        // Credit Vendor
        // ==========================================

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