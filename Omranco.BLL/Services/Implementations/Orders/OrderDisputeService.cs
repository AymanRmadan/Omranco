using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Disputes;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.OrderServices;

public sealed class OrderDisputeService
    : IOrderDisputeService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderDisputeService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<Result<OpenOrderDisputeResponse>> OpenAsync(
        int orderId,
        int currentUserId,
        OpenOrderDisputeRequest request)
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
}