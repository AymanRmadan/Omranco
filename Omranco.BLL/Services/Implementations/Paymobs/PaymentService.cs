using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Payments.Paymob;
using Omranco.BLL.DTOS.Payments.Paymob.Webhooks;
using Omranco.BLL.DTOS.Payments.Refunds;
using Omranco.BLL.DTOS.Payments.Requests;
using Omranco.BLL.DTOS.Payments.Responses;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.BLL.Services.Abstractions.PaymentServices;
using Omranco.BLL.Services.Abstractions.WalletServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Payments;
using Omranco.Domain.Repositories.Abstractions;
using System.Globalization;

namespace Omranco.BLL.Services.Implementations.PaymentServices;

public sealed class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly PaymobOptions _paymobOptions;
    private readonly IPaymobHmacService _paymobHmacService;
    private readonly IOrderSettlementService _orderSettlementService;
    private readonly IVendorWalletService _vendorWalletService;

    public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymobService paymobService,
            IPaymobHmacService paymobHmacService,
            IOrderSettlementService orderSettlementService,
            IOptions<PaymobOptions> paymobOptions,
            IVendorWalletService vendorWalletService
)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _paymobHmacService = paymobHmacService;
        _orderSettlementService = orderSettlementService;
        _paymobOptions = paymobOptions.Value;
        _vendorWalletService = vendorWalletService;
    }


    public async Task<Result<StartPaymentResponse>> StartPaymentAsync(int orderId, int currentUserId, StartPaymentRequest request)
    {
        // ==========================================
        // Get Payment + Order + Project
        // ==========================================

        var payment =
            await _unitOfWork.Payments
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId,
                    x => x.Order,
                    x => x.Order.Project
                );


        if (payment is null)
        {
            return Result
                .Failure<StartPaymentResponse>(
                    PaymentErrors.NotFound
                );
        }


        // ==========================================
        // Customer Access
        // ==========================================

        var accountUser =
            await _unitOfWork
                .CustomerAccountUsers
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == currentUserId
                        &&
                        x.CustomerAccountId ==
                            payment.Order.Project
                                .CustomerAccountId
                        &&
                        x.IsActive
                );


        if (accountUser is null)
        {
            return Result
                .Failure<StartPaymentResponse>(
                    PaymentErrors.AccessDenied
                );
        }


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
                                payment.Order.ProjectId
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
                .Failure<StartPaymentResponse>(
                    PaymentErrors.AccessDenied
                );
        }


        // ==========================================
        // Payment Status
        // ==========================================

        if (payment.Status ==
            PaymentStatus.Paid)
        {
            return Result
                .Failure<StartPaymentResponse>(
                    PaymentErrors.AlreadyPaid
                );
        }


        if (payment.Status !=
            PaymentStatus.Pending)
        {
            return Result
                .Failure<StartPaymentResponse>(
                    PaymentErrors.InvalidStatus
                );
        }


        // ==========================================
        // Prevent multiple active attempts
        // ==========================================

        var now = DateTime.UtcNow;


        var hasActiveAttempt =
            await _unitOfWork
                .PaymentAttempts
                .AnyAsync(
                    x =>
                        x.PaymentId ==
                            payment.Id
                        &&
                        (
                            x.Status ==
                                PaymentAttemptStatus.Pending
                            ||
                            x.Status ==
                                PaymentAttemptStatus.Processing
                        )
                        &&
                        (
                            !x.ExpiresAt.HasValue
                            ||
                            x.ExpiresAt.Value > now
                        )
                );


        if (hasActiveAttempt)
        {
            return Result
                .Failure<StartPaymentResponse>(
                    PaymentErrors.ActiveAttemptExists
                );
        }


        // ==========================================
        // Create our internal reference
        // ==========================================

        var reference =
            $"PAY-{payment.Id}-" +
            $"{Guid.NewGuid():N}";


        // ==========================================
        // Create Payment Attempt
        // ==========================================

        var attempt =
            new PaymentAttempt
            {
                PaymentId =
                    payment.Id,

                Method =
                    request.PaymentMethod,

                Status =
                    PaymentAttemptStatus.Pending,

                Amount =
                    payment.Amount,

                Reference =
                    reference,

                CreatedAt =
                    now,

                ExpiresAt =
                    now.AddSeconds(
                        _paymobOptions
                            .IntentionExpirationSeconds
                    )
            };


        await _unitOfWork
            .PaymentAttempts
            .InsertAsync(attempt);


        /*
         * بنعمل Save هنا قبل Paymob.
         *
         * عشان يبقى عندنا Attempt مسجلة داخليًا
         * قبل ما نكلم External Provider.
         */
        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Create Paymob Intention
        // ==========================================

        var paymobRequest =
            new CreatePaymobIntentionRequest(
                Amount:
                    payment.Amount,

                Currency:
                    payment.Currency,

                PaymentMethod:
                    request.PaymentMethod,

                Reference:
                    reference,

                ItemName:
                    $"Omranco Order {payment.Order.Number}",

                ItemDescription:
                    $"Payment for Omranco order {payment.Order.Number}",

                BillingData:
                    request.BillingData
            );


        var paymobResult =
            await _paymobService
                .CreateIntentionAsync(
                    paymobRequest
                );


        // ==========================================
        // Paymob Failed
        // ==========================================

        if (paymobResult.IsFailure)
        {
            /*
             * attempt لسه Tracked لأننا أنشأناها
             * بنفس DbContext.
             */

            attempt.Status =
                PaymentAttemptStatus.Failed;

            attempt.FailureMessage =
                paymobResult.Error.Description;

            attempt.CompletedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result
                .Failure<StartPaymentResponse>(
                    paymobResult.Error
                );
        }


        var paymobResponse =
            paymobResult.Value!;


        // ==========================================
        // Update Attempt
        // ==========================================

        attempt.Status =
            PaymentAttemptStatus.Processing;

        attempt.PaymobIntentionId =
            paymobResponse.IntentionId;

        attempt.PaymobOrderId =
            paymobResponse.PaymobOrderId;


        await _unitOfWork
            .SaveChangeAsync();


        // ==========================================
        // Return Checkout
        // ==========================================

        var response =
            new StartPaymentResponse(
                PaymentAttemptId:
                    attempt.Id,

                CheckoutUrl:
                    paymobResponse.CheckoutUrl
            );


        return Result.Success(response);
    }


    public async Task<Result> HandlePaymobWebhookAsync(PaymobTransactionCallback callback, string hmac)
    {
        // ==========================================
        // Validate HMAC
        // ==========================================

        var isValidHmac =
            _paymobHmacService
                .IsValidTransactionHmac(
                    callback.Obj,
                    hmac
                );


        if (!isValidHmac)
        {
            return Result.Failure(
                PaymentErrors.InvalidWebhookSignature
            );
        }

        var transaction = callback.Obj;

        if (transaction.IsRefund)
        {
            return await HandleRefundWebhookAsync(
                transaction
            );
        }


        // ==========================================
        // Refund callbacks هنشتغل عليها بعدين
        // ==========================================




        // ==========================================
        // Find Attempt by Paymob Order Id
        // ==========================================

        var paymobOrderId =
            transaction.Order.Id.ToString(
                CultureInfo.InvariantCulture
            );


        var attempt =
            await _unitOfWork
                .PaymentAttempts
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymobOrderId ==
                        paymobOrderId,
                    x => x.Payment
                );


        if (attempt is null)
        {
            return Result.Failure(
                PaymentErrors.AttemptNotFound
            );
        }


        // ==========================================
        // Idempotency
        // ==========================================

        var transactionId =
            transaction.Id.ToString(
                CultureInfo.InvariantCulture
            );


        if (!string.IsNullOrWhiteSpace(
                attempt.PaymobTransactionId)
            &&
            attempt.PaymobTransactionId ==
                transactionId
            &&
            (
                attempt.Status ==
                    PaymentAttemptStatus.Succeeded
                ||
                attempt.Status ==
                    PaymentAttemptStatus.Failed
            ))
        {
            // نفس الـWebhook اتبعت تاني.
            return Result.Success();
        }


        // ==========================================
        // Validate Currency
        // ==========================================

        if (!string.Equals(
                transaction.Currency,
                attempt.Payment.Currency,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                PaymentErrors.CurrencyMismatch
            );
        }


        // ==========================================
        // Validate Amount
        // ==========================================

        var expectedAmountCents =
            decimal.ToInt64(
                decimal.Round(
                    attempt.Amount * 100m,
                    0,
                    MidpointRounding.AwayFromZero
                )
            );


        if (transaction.AmountCents !=
            expectedAmountCents)
        {
            return Result.Failure(
                PaymentErrors.AmountMismatch
            );
        }


        // ==========================================
        // Get Tracked Entities
        // ==========================================

        var trackedAttempt =
            await _unitOfWork
                .PaymentAttempts
                .FindAsync(
                    attempt.Id
                );


        if (trackedAttempt is null)
        {
            return Result.Failure(
                PaymentErrors.AttemptNotFound
            );
        }


        var trackedPayment =
            await _unitOfWork
                .Payments
                .FindAsync(
                    attempt.PaymentId
                );


        if (trackedPayment is null)
        {
            return Result.Failure(
                PaymentErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;


        trackedAttempt.PaymobTransactionId =
            transactionId;

        trackedAttempt.CompletedAt =
            now;


        // ==========================================
        // Payment Succeeded
        // ==========================================

        if (transaction.Success &&
            !transaction.Pending)
        {
            /*
             * Idempotency على مستوى الـPayment نفسها.
             */
            if (trackedPayment.Status == PaymentStatus.Paid)
            {
                trackedAttempt.Status = PaymentAttemptStatus.Succeeded;
                await _unitOfWork.SaveChangeAsync();

                /*
                 * Reconciliation:
                 *
                 * لو Payment كانت اتحفظت Paid
                 * لكن Wallet settlement فشلت قبل كده،
                 * duplicate webhook يصلح الحالة.
                 */
                var setlementResult = await _orderSettlementService
                        .TrySettleAsync(trackedPayment.OrderId);


                if (setlementResult.IsFailure)
                {
                    return Result.Failure(setlementResult.Error);
                }


                return Result.Success();
            }


            trackedAttempt.Status = PaymentAttemptStatus.Succeeded;


            trackedPayment.Status = PaymentStatus.Paid;

            trackedPayment.PaidAmount = trackedPayment.Amount;

            trackedPayment.PaidAt = now;

            trackedPayment.UpdatedAt = now;


            await _unitOfWork.SaveChangeAsync();


            var settlementResult = await _orderSettlementService.TrySettleAsync(trackedPayment.OrderId);


            if (settlementResult.IsFailure)
            {
                return Result.Failure(settlementResult.Error);
            }
            return Result.Success();
        }


        // ==========================================
        // Payment Failed
        // ==========================================

        trackedAttempt.Status = PaymentAttemptStatus.Failed;

        trackedAttempt.FailureMessage = transaction.Data?.Message ?? "Payment failed.";


        /*
         * Payment نفسها تفضل Pending.
         *
         * ليه؟
         *
         * عشان العميل يقدر يعمل Attempt جديدة.
         */
        trackedPayment.Status =
            PaymentStatus.Pending;

        trackedPayment.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    public async Task<Result<RequestRefundResponse>> RequestRefundAsync(int orderId, int currentUserId, RequestRefundRequest request)
    {
        // ==========================================
        // Validate Amount
        // ==========================================

        if (request.Amount <= 0)
        {
            return Result
                .Failure<RequestRefundResponse>(
                    PaymentRefundErrors.InvalidAmount
                );
        }


        // ==========================================
        // Get Payment + Order + Project
        // ==========================================

        var payment =
            await _unitOfWork.Payments
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId,
                    x => x.Order,
                    x => x.Order.Project
                );


        if (payment is null)
        {
            return Result
                .Failure<RequestRefundResponse>(
                    PaymentErrors.NotFound
                );
        }


        // ==========================================
        // Customer Access
        // ==========================================

        var accountUser =
            await _unitOfWork
                .CustomerAccountUsers
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == currentUserId
                        &&
                        x.CustomerAccountId ==
                            payment.Order.Project.CustomerAccountId
                        &&
                        x.IsActive
                );


        if (accountUser is null)
        {
            return Result
                .Failure<RequestRefundResponse>(
                    PaymentErrors.AccessDenied
                );
        }


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
                                payment.Order.ProjectId
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
                .Failure<RequestRefundResponse>(
                    PaymentErrors.AccessDenied
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
                .Failure<RequestRefundResponse>(
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
                .Failure<RequestRefundResponse>(
                    PaymentRefundErrors.ActiveRefundExists
                );
        }


        // ==========================================
        // Calculate remaining refundable amount
        // ==========================================

        var remainingRefundableAmount =
            payment.PaidAmount
            - payment.RefundedAmount;


        if (remainingRefundableAmount <= 0)
        {
            return Result
                .Failure<RequestRefundResponse>(
                    PaymentRefundErrors.PaymentNotRefundable
                );
        }


        if (request.Amount >
            remainingRefundableAmount)
        {
            return Result
                .Failure<RequestRefundResponse>(
                    PaymentRefundErrors.AmountExceedsRefundable
                );
        }


        // ==========================================
        // Find successful Paymob payment attempt
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
                .Failure<RequestRefundResponse>(
                    PaymentRefundErrors
                        .SuccessfulAttemptNotFound
                );
        }


        // ==========================================
        // Create internal refund
        // ==========================================

        var now =
            DateTime.UtcNow;


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
                    PaymentRefundStatus.Pending,

                Reference =
                    $"REF-{payment.Id}-{Guid.NewGuid():N}",

                Reason =
                    request.Reason,

                RequestedByUserId =
                    currentUserId,

                RequestedAt =
                    now
            };


        await _unitOfWork.PaymentRefunds.InsertAsync(refund);

        await _unitOfWork.SaveChangeAsync();



        if (!long.TryParse(successfulAttempt.PaymobTransactionId, out var paymobTransactionId))
        {
            return Result.Failure<RequestRefundResponse>(PaymentRefundErrors.InvalidProviderTransaction);
        }

        // ==========================================
        // Mark as Processing before calling Paymob
        // ==========================================

        refund.Status = PaymentRefundStatus.Processing;

        refund.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangeAsync();


        var paymobRefundResult = await _paymobService.RefundAsync(paymobTransactionId, refund.Amount);

        if (paymobRefundResult.IsFailure)
        {
            /*
             * لو الحالة Unknown بسبب Timeout / Network،
             * ممنوع نحولها Failed.
             *
             * نسيبها Processing.
             */
            if (paymobRefundResult.Error ==
                PaymobErrors.RefundStatusUnknown)
            {
                return Result
                    .Failure<RequestRefundResponse>(
                        paymobRefundResult.Error
                    );
            }


            /*
             * هنا Paymob ردت علينا برفض واضح.
             */
            refund.Status = PaymentRefundStatus.Failed;

            refund.FailureMessage = paymobRefundResult.Error.Description;

            refund.CompletedAt = DateTime.UtcNow;

            refund.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangeAsync();

            return Result.Failure<RequestRefundResponse>(paymobRefundResult.Error);
        }


        // ==========================================
        // Response
        // ==========================================

        var response =
            new RequestRefundResponse(RefundId: refund.Id, Amount: refund.Amount,
            RemainingRefundableAmount: remainingRefundableAmount - refund.Amount);


        return Result.Success(response);
    }


    private async Task<Result> HandleRefundWebhookAsync(PaymobTransactionObject transaction)
    {
        // ==========================================
        // Original Paymob Transaction
        // ==========================================

        if (string.IsNullOrWhiteSpace(
                transaction.ParentTransaction))
        {
            return Result.Failure(
                PaymentRefundErrors
                    .InvalidProviderTransaction
            );
        }


        var parentTransactionId =
            transaction.ParentTransaction;


        // ==========================================
        // Find original successful payment attempt
        // ==========================================

        var paymentAttempt =
            await _unitOfWork
                .PaymentAttempts
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymobTransactionId ==
                            parentTransactionId
                        &&
                        x.Status ==
                            PaymentAttemptStatus.Succeeded
                );


        if (paymentAttempt is null)
        {
            return Result.Failure(
                PaymentRefundErrors
                    .SuccessfulAttemptNotFound
            );
        }


        // ==========================================
        // Refund Transaction Id
        // ==========================================

        var refundTransactionId =
            transaction.Id.ToString(
                CultureInfo.InvariantCulture
            );


        // ==========================================
        // Convert Paymob Amount
        // cents / piastres -> EGP
        // ==========================================

        var refundAmount =
            transaction.AmountCents / 100m;


        // ==========================================
        // Find our Processing Refund
        // ==========================================

        var refund =
            await _unitOfWork
                .PaymentRefunds
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymentId ==
                            paymentAttempt.PaymentId
                        &&
                        x.PaymentAttemptId ==
                            paymentAttempt.Id
                        &&
                        x.Status ==
                            PaymentRefundStatus.Processing
                        &&
                        x.Amount ==
                            refundAmount
                );


        // ==========================================
        // Refund not Processing?
        // Check if this webhook was already processed
        // ==========================================

        if (refund is null)
        {
            var completedRefund =
                await _unitOfWork
                    .PaymentRefunds
                    .FirstOrDefaultAsync(
                        x =>
                            x.PaymentId ==
                                paymentAttempt.PaymentId
                            &&
                            x.PaymentAttemptId ==
                                paymentAttempt.Id
                            &&
                            x.PaymobRefundTransactionId ==
                                refundTransactionId
                    );


            if (completedRefund is not null)
            {
                if (completedRefund.Status == PaymentRefundStatus.Succeeded)
                {
                    var walletReult = await _vendorWalletService.ApplyRefundAsync(completedRefund.Id);


                    if (walletReult.IsFailure)
                    {
                        return Result.Failure(
                            walletReult.Error
                        );
                    }
                }


                return Result.Success();
            }


            return Result.Failure(
                PaymentRefundErrors.RefundNotFound
            );
        }


        // ==========================================
        // Pending callback
        // ==========================================

        if (transaction.Pending)
        {
            /*
             * Pending does NOT mean Failed.
             *
             * Keep:
             * PaymentRefund = Processing
             */
            return Result.Success();
        }


        // ==========================================
        // Get tracked Refund
        // ==========================================

        var trackedRefund =
            await _unitOfWork
                .PaymentRefunds
                .FindAsync(
                    refund.Id
                );


        if (trackedRefund is null)
        {
            return Result.Failure(
                PaymentRefundErrors.RefundNotFound
            );
        }


        var now =
            DateTime.UtcNow;


        // ==========================================
        // Refund Failed
        // ==========================================

        if (!transaction.Success)
        {
            trackedRefund.Status =
                PaymentRefundStatus.Failed;

            trackedRefund.PaymobRefundTransactionId =
                refundTransactionId;

            trackedRefund.FailureMessage =
                transaction.Data?.Message
                ?? "Refund failed.";

            trackedRefund.CompletedAt =
                now;

            trackedRefund.UpdatedAt =
                now;


            // ======================================
            // Cancellation Refund Failed?
            // ======================================

            var cancellation =
                await _unitOfWork
                    .OrderCancellations
                    .FirstOrDefaultAsync(
                        x =>
                            x.PaymentRefundId ==
                                trackedRefund.Id
                            &&
                            x.Status ==
                                OrderCancellationStatus
                                    .RefundProcessing
                    );


            if (cancellation is not null)
            {
                var trackedCancellation =
                    await _unitOfWork
                        .OrderCancellations
                        .FindAsync(
                            cancellation.Id
                        );


                if (trackedCancellation is not null)
                {
                    trackedCancellation.Status =
                        OrderCancellationStatus.Failed;

                    trackedCancellation.FailureReason =
                        trackedRefund.FailureMessage;

                    trackedCancellation.UpdatedAt =
                        now;
                }
            }


            // ======================================
            // Dispute Refund Failed?
            // ======================================

            var dispute =
                await _unitOfWork
                    .OrderDisputes
                    .FirstOrDefaultAsync(
                        x =>
                            x.PaymentRefundId ==
                                trackedRefund.Id
                            &&
                            x.Status ==
                                OrderDisputeStatus
                                    .RefundProcessing
                    );


            if (dispute is not null)
            {
                var trackedDispute =
                    await _unitOfWork
                        .OrderDisputes
                        .FindAsync(
                            dispute.Id
                        );


                if (trackedDispute is not null)
                {
                    trackedDispute.Status =
                        OrderDisputeStatus.RefundFailed;

                    trackedDispute.UpdatedAt =
                        now;
                }
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // ==========================================
        // Refund Succeeded
        // ==========================================

        var trackedPayment =
            await _unitOfWork
                .Payments
                .FindAsync(
                    paymentAttempt.PaymentId
                );


        if (trackedPayment is null)
        {
            return Result.Failure(
                PaymentErrors.NotFound
            );
        }


        // ==========================================
        // Safety - calculate new refunded amount
        // ==========================================

        var newRefundedAmount =
            trackedPayment.RefundedAmount
            +
            trackedRefund.Amount;


        if (newRefundedAmount >
            trackedPayment.PaidAmount)
        {
            return Result.Failure(
                PaymentRefundErrors
                    .AmountExceedsRefundable
            );
        }


        // ==========================================
        // Complete Refund
        // ==========================================

        trackedRefund.Status =
            PaymentRefundStatus.Succeeded;

        trackedRefund.PaymobRefundTransactionId =
            refundTransactionId;

        trackedRefund.FailureCode =
            null;

        trackedRefund.FailureMessage =
            null;

        trackedRefund.CompletedAt =
            now;

        trackedRefund.UpdatedAt =
            now;


        // ==========================================
        // Update Payment
        // ==========================================

        trackedPayment.RefundedAmount =
            newRefundedAmount;

        trackedPayment.UpdatedAt =
            now;


        if (trackedPayment.RefundedAmount >=
            trackedPayment.PaidAmount)
        {
            trackedPayment.Status =
                PaymentStatus.Refunded;
        }
        else
        {
            trackedPayment.Status =
                PaymentStatus.PartiallyRefunded;
        }


        // ==========================================
        // Is this Refund related to Cancellation?
        // ==========================================

        var orderCancellation =
            await _unitOfWork
                .OrderCancellations
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymentRefundId ==
                            trackedRefund.Id
                        &&
                        x.Status ==
                            OrderCancellationStatus
                                .RefundProcessing
                );


        if (orderCancellation is not null)
        {
            /*
             * Cancellation refund should refund
             * the full remaining amount.
             *
             * Therefore we only complete cancellation
             * when Payment itself became Refunded.
             */
            if (trackedPayment.Status ==
                PaymentStatus.Refunded)
            {
                var trackedCancellation =
                    await _unitOfWork
                        .OrderCancellations
                        .FindAsync(
                            orderCancellation.Id
                        );


                var trackedOrder =
                    await _unitOfWork
                        .Orders
                        .FindAsync(
                            trackedPayment.OrderId
                        );


                if (trackedCancellation is not null
                    &&
                    trackedOrder is not null)
                {
                    var oldOrderStatus =
                        trackedOrder.Status;


                    // ==================================
                    // Complete Cancellation
                    // ==================================

                    trackedCancellation.Status =
                        OrderCancellationStatus.Completed;

                    trackedCancellation.CompletedAt =
                        now;

                    trackedCancellation.UpdatedAt =
                        now;


                    // ==================================
                    // Cancel Order
                    // ==================================

                    trackedOrder.Status =
                        OrderStatus.Cancelled;

                    trackedOrder.CancelledAt =
                        now;

                    trackedOrder.UpdatedAt =
                        now;


                    // ==================================
                    // Order Status History
                    // ==================================

                    await _unitOfWork
                        .OrderStatusHistory
                        .InsertAsync(
                            new OrderStatusHistory
                            {
                                OrderId =
                                    trackedOrder.Id,

                                OldStatus =
                                    oldOrderStatus,

                                NewStatus =
                                    OrderStatus.Cancelled,

                                ChangedByUserId =
                                    trackedCancellation
                                        .RequestedByUserId,

                                ChangedAt =
                                    now
                            }
                        );
                }
            }
        }


        // ==========================================
        // Is this Refund related to a Dispute?
        // ==========================================

        var orderDispute =
            await _unitOfWork
                .OrderDisputes
                .FirstOrDefaultAsync(
                    x =>
                        x.PaymentRefundId ==
                            trackedRefund.Id
                        &&
                        x.Status ==
                            OrderDisputeStatus
                                .RefundProcessing
                );


        if (orderDispute is not null)
        {
            var trackedDispute =
                await _unitOfWork
                    .OrderDisputes
                    .FindAsync(
                        orderDispute.Id
                    );


            if (trackedDispute is not null)
            {
                /*
                 * Dispute ممكن تكون Partial Refund.
                 *
                 * مش شرط Payment كلها تبقى Refunded.
                 * نجاح الـRefund الموافق عليه كفاية
                 * إن النزاع نفسه يبقى Resolved.
                 */
                trackedDispute.Status =
                    OrderDisputeStatus.Resolved;

                trackedDispute.ResolvedAt =
                    now;

                trackedDispute.UpdatedAt =
                    now;
            }
        }


        // ==========================================
        // Save everything
        // ==========================================

        await _unitOfWork
     .SaveChangeAsync();


        // ==========================================
        // Sync Vendor Wallet
        // ==========================================

        var walletResult =
            await _vendorWalletService
                .ApplyRefundAsync(
                    trackedRefund.Id
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