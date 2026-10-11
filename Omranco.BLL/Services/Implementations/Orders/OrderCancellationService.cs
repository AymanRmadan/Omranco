using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Cancellations;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.BLL.Services.Abstractions.PaymentServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Payments;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.OrderServices;

public sealed class OrderCancellationService
    : IOrderCancellationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;

    public OrderCancellationService(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
    }


    public async Task<Result<CancelOrderResponse>> CancelAsync(
        int orderId,
        int currentUserId,
        CancelOrderRequest request)
    {
        var order =
            await _unitOfWork.Orders
                .FirstOrDefaultAsync(
                    x => x.Id == orderId,
                    x => x.Project,
                    x => x.Payment
                );

        if (order is null)
        {
            return Result.Failure<CancelOrderResponse>(
                OrderErrors.NotFound
            );
        }


        // ==========================================
        // Customer Access
        // ==========================================

        var accountUser =
            await _unitOfWork.CustomerAccountUsers
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == currentUserId
                        &&
                        x.CustomerAccountId ==
                            order.Project.CustomerAccountId
                        &&
                        x.IsActive
                );

        if (accountUser is null)
        {
            return Result.Failure<CancelOrderResponse>(
                PaymentErrors.AccessDenied
            );
        }


        var hasAccess =
            accountUser.Role ==
            CustomerAccountUserRole.Owner;


        if (!hasAccess)
        {
            hasAccess =
                await _unitOfWork.ProjectMembers
                    .AnyAsync(
                        x =>
                            x.ProjectId == order.ProjectId
                            &&
                            x.CustomerAccountUserId ==
                                accountUser.Id
                            &&
                            x.IsActive
                    );
        }


        if (!hasAccess)
        {
            return Result.Failure<CancelOrderResponse>(
                PaymentErrors.AccessDenied
            );
        }


        // ==========================================
        // Allowed Order Status
        // ==========================================

        if (order.Status != OrderStatus.Confirmed
            &&
            order.Status != OrderStatus.InProgress)
        {
            return Result.Failure<CancelOrderResponse>(
                OrderCancellationErrors.NotAllowed
            );
        }


        var cancellationExists =
            await _unitOfWork.OrderCancellations
                .AnyAsync(
                    x => x.OrderId == orderId
                );

        if (cancellationExists)
        {
            return Result.Failure<CancelOrderResponse>(
                OrderCancellationErrors.AlreadyRequested
            );
        }


        if (order.Payment is null)
        {
            return Result.Failure<CancelOrderResponse>(
                PaymentErrors.NotFound
            );
        }


        var now = DateTime.UtcNow;

        var cancellation =
            new OrderCancellation
            {
                OrderId = order.Id,

                RequestedByUserId =
                    currentUserId,

                Reason =
                    request.Reason,

                Status =
                    OrderCancellationStatus.Requested,

                RequestedAt =
                    now
            };


        // ==========================================
        // Payment never completed
        // ==========================================

        if (order.Payment.Status ==
            PaymentStatus.Pending)
        {
            cancellation.Status =
                OrderCancellationStatus.Completed;

            cancellation.CompletedAt =
                now;


            var trackedOrder =
                await _unitOfWork.Orders
                    .FindAsync(order.Id);


            var trackedPayment =
                await _unitOfWork.Payments
                    .FindAsync(order.Payment.Id);


            if (trackedOrder is null ||
                trackedPayment is null)
            {
                return Result.Failure<CancelOrderResponse>(
                    OrderErrors.NotFound
                );
            }


            var oldStatus =
                trackedOrder.Status;


            trackedOrder.Status =
                OrderStatus.Cancelled;

            trackedOrder.CancelledAt =
                now;

            trackedOrder.UpdatedAt =
                now;


            trackedPayment.Status =
                PaymentStatus.Cancelled;

            trackedPayment.UpdatedAt =
                now;


            await _unitOfWork.OrderCancellations
                .InsertAsync(cancellation);


            await _unitOfWork.OrderStatusHistory
                .InsertAsync(
                    new OrderStatusHistory
                    {
                        OrderId =
                            order.Id,

                        OldStatus =
                            oldStatus,

                        NewStatus =
                            OrderStatus.Cancelled,

                        ChangedByUserId =
                            currentUserId,

                        ChangedAt =
                            now
                    }
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                new CancelOrderResponse(
                    cancellation.Id,
                    cancellation.Status,
                    null
                )
            );
        }


        // ==========================================
        // Already completely refunded
        // ==========================================

        if (order.Payment.Status ==
            PaymentStatus.Refunded)
        {
            cancellation.Status =
                OrderCancellationStatus.Completed;

            cancellation.CompletedAt =
                now;


            var trackedOrder =
                await _unitOfWork.Orders
                    .FindAsync(order.Id);


            if (trackedOrder is null)
            {
                return Result.Failure<CancelOrderResponse>(
                    OrderErrors.NotFound
                );
            }


            var oldStatus =
                trackedOrder.Status;


            trackedOrder.Status =
                OrderStatus.Cancelled;

            trackedOrder.CancelledAt =
                now;

            trackedOrder.UpdatedAt =
                now;


            await _unitOfWork.OrderCancellations
                .InsertAsync(cancellation);


            await _unitOfWork.OrderStatusHistory
                .InsertAsync(
                    new OrderStatusHistory
                    {
                        OrderId =
                            order.Id,

                        OldStatus =
                            oldStatus,

                        NewStatus =
                            OrderStatus.Cancelled,

                        ChangedByUserId =
                            currentUserId,

                        ChangedAt =
                            now
                    }
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                new CancelOrderResponse(
                    cancellation.Id,
                    cancellation.Status,
                    null
                )
            );
        }


        // ==========================================
        // Paid payment requires Refund
        // ==========================================

        if (order.Payment.Status != PaymentStatus.Paid
            &&
            order.Payment.Status !=
                PaymentStatus.PartiallyRefunded)
        {
            return Result.Failure<CancelOrderResponse>(
                OrderCancellationErrors
                    .PaymentNotRefundable
            );
        }


        var remainingRefundAmount =
            order.Payment.PaidAmount
            -
            order.Payment.RefundedAmount;


        if (remainingRefundAmount <= 0)
        {
            return Result.Failure<CancelOrderResponse>(
                OrderCancellationErrors
                    .PaymentNotRefundable
            );
        }


        var hasActiveRefund =
            await _unitOfWork.PaymentRefunds
                .AnyAsync(
                    x =>
                        x.PaymentId ==
                            order.Payment.Id
                        &&
                        (
                            x.Status ==
                                PaymentRefundStatus.Pending
                            ||
                            x.Status ==
                                PaymentRefundStatus.Processing
                        )
                );


        if (hasActiveRefund)
        {
            return Result.Failure<CancelOrderResponse>(
                PaymentRefundErrors.ActiveRefundExists
            );
        }


        var successfulAttempt =
            await _unitOfWork.PaymentAttempts
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymentId ==
                            order.Payment.Id
                        &&
                        x.Status ==
                            PaymentAttemptStatus.Succeeded
                        &&
                        x.PaymobTransactionId != null
                );


        if (successfulAttempt is null)
        {
            return Result.Failure<CancelOrderResponse>(
                PaymentRefundErrors
                    .SuccessfulAttemptNotFound
            );
        }


        if (!long.TryParse(
                successfulAttempt.PaymobTransactionId,
                out var paymobTransactionId))
        {
            return Result.Failure<CancelOrderResponse>(
                PaymentRefundErrors
                    .InvalidProviderTransaction
            );
        }


        // ==========================================
        // Create Refund
        // ==========================================

        var refund =
            new PaymentRefund
            {
                PaymentId =
                    order.Payment.Id,

                PaymentAttemptId =
                    successfulAttempt.Id,

                Amount =
                    remainingRefundAmount,

                Status =
                    PaymentRefundStatus.Processing,

                Reference =
                    $"CANCEL-{order.Id}-{Guid.NewGuid():N}",

                Reason =
                    request.Reason
                    ?? $"Cancellation of order {order.Number}",

                RequestedByUserId =
                    currentUserId,

                RequestedAt =
                    now,

                UpdatedAt =
                    now
            };


        await _unitOfWork.PaymentRefunds
            .InsertAsync(refund);


        /*
         * Save الأول عشان يبقى عندنا Refund record
         * قبل الاتصال بـ Paymob.
         */
        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Create Cancellation
        // ==========================================

        cancellation.Status =
            OrderCancellationStatus.RefundProcessing;

        cancellation.PaymentRefundId =
            refund.Id;


        await _unitOfWork.OrderCancellations
            .InsertAsync(cancellation);


        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Paymob Refund
        // ==========================================

        var refundResult =
            await _paymobService
                .RefundAsync(
                    paymobTransactionId,
                    remainingRefundAmount
                );


        if (refundResult.IsFailure)
        {
            /*
             * Timeout / Network:
             *
             * سيب Refund وCancellation Processing
             * لأننا مش عارفين هل Paymob استلمت
             * العملية أم لا.
             */
            if (refundResult.Error.Code ==
                PaymobErrors
                    .RefundStatusUnknown
                    .Code)
            {
                return Result
                    .Failure<CancelOrderResponse>(
                        refundResult.Error
                    );
            }


            var trackedRefund =
                await _unitOfWork.PaymentRefunds
                    .FindAsync(refund.Id);


            var trackedCancellation =
                await _unitOfWork.OrderCancellations
                    .FindAsync(cancellation.Id);


            if (trackedRefund is not null)
            {
                trackedRefund.Status =
                    PaymentRefundStatus.Failed;

                trackedRefund.FailureMessage =
                    refundResult.Error.Description;

                trackedRefund.CompletedAt =
                    DateTime.UtcNow;

                trackedRefund.UpdatedAt =
                    DateTime.UtcNow;
            }


            if (trackedCancellation is not null)
            {
                trackedCancellation.Status =
                    OrderCancellationStatus.Failed;

                trackedCancellation.FailureReason =
                    refundResult.Error.Description;

                trackedCancellation.UpdatedAt =
                    DateTime.UtcNow;
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result
                .Failure<CancelOrderResponse>(
                    refundResult.Error
                );
        }


        // ==========================================
        // Wait for Paymob webhook
        // ==========================================

        return Result.Success(
            new CancelOrderResponse(
                cancellation.Id,
                cancellation.Status,
                refund.Id
            )
        );
    }
}