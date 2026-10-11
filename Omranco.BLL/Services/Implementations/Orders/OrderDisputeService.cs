using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Disputes;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.BLL.Services.Abstractions.PaymentServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Payments;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.OrderServices;

public sealed class OrderDisputeService
    : IOrderDisputeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;

    public OrderDisputeService(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
    }


    public async Task<Result<OpenOrderDisputeResponse>> OpenAsync(int orderId, int currentUserId, OpenOrderDisputeRequest request)
    {
        // ==========================================
        // Validate Reason
        // ==========================================

        if (string.IsNullOrWhiteSpace(
                request.Reason))
        {
            return Result
                .Failure<OpenOrderDisputeResponse>(
                    OrderDisputeErrors.ReasonRequired
                );
        }


        // ==========================================
        // Get Order + Project
        // ==========================================

        var order =
            await _unitOfWork.Orders
                .FirstOrDefaultAsync(
                    x => x.Id == orderId,
                    x => x.Project
                );


        if (order is null)
        {
            return Result
                .Failure<OpenOrderDisputeResponse>(
                    OrderErrors.NotFound
                );
        }


        // ==========================================
        // Order Status
        // ==========================================

        if (order.Status != OrderStatus.Shipped
            &&
            order.Status != OrderStatus.Delivered)
        {
            return Result
                .Failure<OpenOrderDisputeResponse>(
                    OrderDisputeErrors.InvalidOrderStatus
                );
        }


        // ==========================================
        // Customer Account User
        // ==========================================

        var accountUser =
            await _unitOfWork
                .CustomerAccountUsers
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
            return Result
                .Failure<OpenOrderDisputeResponse>(
                    OrderDisputeErrors.AccessDenied
                );
        }


        // ==========================================
        // Project Access
        // ==========================================

        var hasProjectAccess =
            accountUser.Role ==
            CustomerAccountUserRole.Owner;


        if (!hasProjectAccess)
        {
            hasProjectAccess =
                await _unitOfWork
                    .ProjectMembers
                    .AnyAsync(
                        x =>
                            x.ProjectId ==
                                order.ProjectId
                            &&
                            x.CustomerAccountUserId ==
                                accountUser.Id
                            &&
                            x.IsActive
                    );
        }


        if (!hasProjectAccess)
        {
            return Result
                .Failure<OpenOrderDisputeResponse>(
                    OrderDisputeErrors.AccessDenied
                );
        }


        // ==========================================
        // Prevent more than one active dispute
        // ==========================================

        var hasActiveDispute =
            await _unitOfWork
                .OrderDisputes
                .AnyAsync(
                    x =>
                        x.OrderId == orderId
                        &&
                        (
                            x.Status ==
                                OrderDisputeStatus.Open
                            ||
                            x.Status ==
                                OrderDisputeStatus.UnderReview
                            ||
                            x.Status ==
                                OrderDisputeStatus.RefundProcessing
                            ||
                            x.Status == OrderDisputeStatus.RefundFailed
                        )
                );


        if (hasActiveDispute)
        {
            return Result
                .Failure<OpenOrderDisputeResponse>(
                    OrderDisputeErrors.ActiveDisputeExists
                );
        }


        // ==========================================
        // Create Dispute
        // ==========================================

        var now =
            DateTime.UtcNow;


        var dispute =
            new OrderDispute
            {
                OrderId =
                    order.Id,

                OpenedByAccountUserId =
                    accountUser.Id,

                Type =
                    request.Type,

                Status =
                    OrderDisputeStatus.Open,

                Reason =
                    request.Reason.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(
                        request.Description)
                        ? null
                        : request.Description.Trim(),

                OpenedAt =
                    now
            };


        await _unitOfWork
            .OrderDisputes
            .InsertAsync(dispute);


        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Response
        // ==========================================

        return Result.Success(
            new OpenOrderDisputeResponse(
                DisputeId:
                    dispute.Id,

                Type:
                    dispute.Type,

                Status:
                    dispute.Status,

                OpenedAt:
                    dispute.OpenedAt
            )
        );
    }

    public async Task<Result<OrderDisputeReviewResponse>> StartReviewAsync(int disputeId, int currentUserId)
    {
        // ==========================================
        // Get Dispute
        // ==========================================

        var dispute =
            await _unitOfWork
                .OrderDisputes
                .GetByIdAsync(disputeId);


        if (dispute is null)
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors.NotFound
                );
        }


        // ==========================================
        // Only Open disputes can start review
        // ==========================================

        if (dispute.Status !=
            OrderDisputeStatus.Open)
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors.InvalidStatus
                );
        }


        // ==========================================
        // Get tracked entity
        // ==========================================

        var trackedDispute =
            await _unitOfWork
                .OrderDisputes
                .FindAsync(dispute.Id);


        if (trackedDispute is null)
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors.NotFound
                );
        }


        // ==========================================
        // Start Review
        // ==========================================

        var now =
            DateTime.UtcNow;


        trackedDispute.Status =
            OrderDisputeStatus.UnderReview;

        trackedDispute.ReviewedByUserId =
            currentUserId;

        trackedDispute.UpdatedAt =
            now;


        /*
         * ReviewedAt مش هنحطها هنا.
         *
         * دي هتمثل وقت القرار النهائي:
         * Reject / Approve.
         */


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            new OrderDisputeReviewResponse(
                DisputeId:
                    trackedDispute.Id,

                Status:
                    trackedDispute.Status,

                ReviewedByUserId:
                    trackedDispute.ReviewedByUserId,

                ReviewedAt:
                    trackedDispute.ReviewedAt
            )
        );
    }

    public async Task<Result<OrderDisputeReviewResponse>> RejectAsync(int disputeId, int currentUserId, RejectOrderDisputeRequest request)
    {
        // ==========================================
        // Validate Notes
        // ==========================================

        if (string.IsNullOrWhiteSpace(
                request.ReviewNotes))
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors
                        .ReviewNotesRequired
                );
        }


        // ==========================================
        // Get Dispute
        // ==========================================

        var dispute =
            await _unitOfWork
                .OrderDisputes
                .GetByIdAsync(disputeId);


        if (dispute is null)
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors.NotFound
                );
        }


        // ==========================================
        // Must be UnderReview
        // ==========================================

        if (dispute.Status !=
            OrderDisputeStatus.UnderReview)
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors.InvalidStatus
                );
        }


        // ==========================================
        // Get tracked entity
        // ==========================================

        var trackedDispute =
            await _unitOfWork
                .OrderDisputes
                .FindAsync(dispute.Id);


        if (trackedDispute is null)
        {
            return Result
                .Failure<OrderDisputeReviewResponse>(
                    OrderDisputeErrors.NotFound
                );
        }


        // ==========================================
        // Reject
        // ==========================================

        var now =
            DateTime.UtcNow;


        trackedDispute.Status =
            OrderDisputeStatus.Rejected;

        trackedDispute.ReviewedByUserId =
            currentUserId;

        trackedDispute.ReviewNotes =
            request.ReviewNotes.Trim();

        trackedDispute.ReviewedAt =
            now;

        trackedDispute.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            new OrderDisputeReviewResponse(
                DisputeId:
                    trackedDispute.Id,

                Status:
                    trackedDispute.Status,

                ReviewedByUserId:
                    trackedDispute.ReviewedByUserId,

                ReviewedAt:
                    trackedDispute.ReviewedAt
            )
        );
    }

    public async Task<Result<ApproveDisputeRefundResponse>> ApproveRefundAsync(int disputeId, int currentUserId, ApproveDisputeRefundRequest request)
    {
        // ==========================================
        // Validate requested amount
        // ==========================================

        if (request.Amount <= 0)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    OrderDisputeErrors.InvalidRefundAmount
                );
        }


        // ==========================================
        // Get dispute + order
        // ==========================================

        var dispute =
            await _unitOfWork
                .OrderDisputes
                .FirstOrDefaultAsync(
                    x => x.Id == disputeId,
                    x => x.Order
                );


        if (dispute is null)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    OrderDisputeErrors.NotFound
                );
        }


        // ==========================================
        // Must be under review
        // ==========================================

        if (dispute.Status !=
            OrderDisputeStatus.UnderReview)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    OrderDisputeErrors.InvalidStatus
                );
        }


        // ==========================================
        // Get payment
        // ==========================================

        var payment =
            await _unitOfWork
                .Payments
                .FirstOrDefaultAsync(
                    x => x.OrderId == dispute.OrderId
                );


        if (payment is null)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    PaymentErrors.NotFound
                );
        }


        // ==========================================
        // Payment must be refundable
        // ==========================================

        if (payment.Status != PaymentStatus.Paid
            &&
            payment.Status != PaymentStatus.PartiallyRefunded)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    PaymentRefundErrors.PaymentNotRefundable
                );
        }


        // ==========================================
        // Prevent concurrent refunds
        // ==========================================

        var hasActiveRefund =
            await _unitOfWork
                .PaymentRefunds
                .AnyAsync(
                    x =>
                        x.PaymentId == payment.Id
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
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    PaymentRefundErrors.ActiveRefundExists
                );
        }


        // ==========================================
        // Remaining refundable amount
        // ==========================================

        var remainingRefundableAmount =
            payment.PaidAmount
            -
            payment.RefundedAmount;


        if (remainingRefundableAmount <= 0)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    PaymentRefundErrors.PaymentNotRefundable
                );
        }


        if (request.Amount >
            remainingRefundableAmount)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    OrderDisputeErrors
                        .RefundAmountExceedsAvailable
                );
        }


        // ==========================================
        // Find original successful payment attempt
        // ==========================================

        var successfulAttempt =
            await _unitOfWork
                .PaymentAttempts
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymentId == payment.Id
                        &&
                        x.Status ==
                            PaymentAttemptStatus.Succeeded
                        &&
                        x.PaymobTransactionId != null
                );


        if (successfulAttempt is null)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    PaymentRefundErrors
                        .SuccessfulAttemptNotFound
                );
        }


        if (!long.TryParse(
                successfulAttempt.PaymobTransactionId,
                out var paymobTransactionId))
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    PaymentRefundErrors
                        .InvalidProviderTransaction
                );
        }


        // ==========================================
        // Get tracked dispute
        // ==========================================

        var trackedDispute =
            await _unitOfWork
                .OrderDisputes
                .FindAsync(dispute.Id);


        if (trackedDispute is null)
        {
            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    OrderDisputeErrors.NotFound
                );
        }


        var now =
            DateTime.UtcNow;


        // ==========================================
        // Create refund
        // ==========================================

        var refund =
            new PaymentRefund
            {
                PaymentId =
                    payment.Id,

                PaymentAttemptId =
                    successfulAttempt.Id,

                Amount =
                    request.Amount,

                Status =
                    PaymentRefundStatus.Processing,

                Reference =
                    $"DISPUTE-{dispute.Id}-{Guid.NewGuid():N}",

                Reason =
                    $"Approved refund for dispute #{dispute.Id}",

                RequestedByUserId =
                    currentUserId,

                RequestedAt =
                    now,

                UpdatedAt =
                    now
            };


        await _unitOfWork
            .PaymentRefunds
            .InsertAsync(refund);


        /*
         * Save الأول عشان refund.Id يتولد
         * ويبقى عندنا record قبل ما نكلم Paymob.
         */
        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Update dispute
        // ==========================================

        trackedDispute.Status =
            OrderDisputeStatus.RefundProcessing;

        trackedDispute.ApprovedRefundAmount =
            request.Amount;

        trackedDispute.PaymentRefundId =
            refund.Id;

        trackedDispute.ReviewedByUserId =
            currentUserId;

        trackedDispute.ReviewNotes =
            string.IsNullOrWhiteSpace(
                request.ReviewNotes)
                ? null
                : request.ReviewNotes.Trim();

        trackedDispute.ReviewedAt =
            now;

        trackedDispute.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Request refund from Paymob
        // ==========================================

        var paymobResult =
            await _paymobService
                .RefundAsync(
                    paymobTransactionId,
                    refund.Amount
                );


        if (paymobResult.IsFailure)
        {
            /*
             * Timeout / network:
             *
             * مينفعش نعتبر العملية Failed
             * لأن Paymob ممكن تكون استلمتها بالفعل.
             */
            if (paymobResult.Error.Code ==
                PaymobErrors
                    .RefundStatusUnknown
                    .Code)
            {
                return Result
                    .Failure<ApproveDisputeRefundResponse>(
                        paymobResult.Error
                    );
            }


            /*
             * Paymob rejected the refund explicitly.
             */
            var trackedRefund =
                await _unitOfWork
                    .PaymentRefunds
                    .FindAsync(refund.Id);


            if (trackedRefund is not null)
            {
                trackedRefund.Status =
                    PaymentRefundStatus.Failed;

                trackedRefund.FailureMessage =
                    paymobResult.Error.Description;

                trackedRefund.CompletedAt =
                    DateTime.UtcNow;

                trackedRefund.UpdatedAt =
                    DateTime.UtcNow;
            }


            trackedDispute.Status =
                OrderDisputeStatus.RefundFailed;

            trackedDispute.UpdatedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result
                .Failure<ApproveDisputeRefundResponse>(
                    paymobResult.Error
                );
        }


        // ==========================================
        // Paymob accepted request.
        // Final result comes from webhook.
        // ==========================================

        return Result.Success(
            new ApproveDisputeRefundResponse(
                DisputeId:
                    trackedDispute.Id,

                RefundId:
                    refund.Id,

                ApprovedRefundAmount:
                    refund.Amount,

                Status:
                    trackedDispute.Status
            )
        );
    }
}