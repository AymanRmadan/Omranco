using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Responses;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.OrderServices
{
    public sealed class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderSettlementService _orderSettlementService;

        public OrderService(
            IUnitOfWork unitOfWork, IOrderSettlementService orderSettlementService)
        {
            _unitOfWork = unitOfWork;
            _orderSettlementService = orderSettlementService;
        }


        public async Task<Result<VendorOrderDetailsResponse>> GetVendorOrderDetailsAsync(int orderId, int currentUserId)
        {
            // ========================================
            // Current Vendor
            // ========================================

            var vendor =
                await _unitOfWork.Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId &&
                            x.IsActive
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorOrderDetailsResponse>(
                        OrderErrors.VendorNotFound
                    );
            }


            // ========================================
            // Order
            // ========================================


            var order =
                await _unitOfWork.Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == orderId &&
                            x.VendorId == vendor.Id,

                        x => x.Project,
                        x => x.Items,
                        x => x.AcceptedByAccountUser,
                        x => x.AcceptedByAccountUser.User
                    );


            if (order is null)
            {
                return Result
                    .Failure<VendorOrderDetailsResponse>(
                        OrderErrors.NotFound
                    );
            }


            // ========================================
            // Catalog Data
            // ========================================

            var productIds =
                order.Items
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();


            var unitIds =
                order.Items
                    .Select(x => x.UnitId)
                    .Distinct()
                    .ToList();


            var variantIds =
                order.Items
                    .Where(x =>
                        x.ProductVariantId.HasValue)
                    .Select(x =>
                        x.ProductVariantId!.Value)
                    .Distinct()
                    .ToList();


            var products =
                await _unitOfWork.Products
                    .GetAllAsync(
                        x => productIds.Contains(x.Id)
                    );


            var units =
                await _unitOfWork.Units
                    .GetAllAsync(
                        x => unitIds.Contains(x.Id)
                    );


            var variants =
                variantIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .ProductVariants
                        .GetAllAsync(
                            x =>
                                variantIds.Contains(
                                    x.Id
                                )
                        );


            var productLookup =
                products.ToDictionary(
                    x => x.Id,
                    x => x.Name
                );


            var unitLookup =
                units.ToDictionary(
                    x => x.Id,
                    x => x.Name
                );


            var variantLookup =
                variants.ToDictionary(
                    x => x.Id,
                    x => x.Name
                );


            // ========================================
            // Response
            // ========================================


            var response = new VendorOrderDetailsResponse(
                order.Id,
                order.Number,
                order.Status.ToString(),
                order.Project.Name,
                order.Project.Address,
                order.AcceptedByAccountUser.User.Name,
                order.AcceptedByAccountUser.User.PhoneNumber,
                order.Subtotal,
                order.ShippingCost,
                order.TotalAmount,
                order.CreatedAt,
                order.DeliveredAt,
                order.Items
                    .Select(item =>
                        new VendorOrderItemResponse(
                            item.ProductId,
                            productLookup.GetValueOrDefault(item.ProductId)
                                ?? string.Empty,
                            item.ProductVariantId,
                            item.ProductVariantId.HasValue
                                ? variantLookup.GetValueOrDefault(
                                    item.ProductVariantId.Value)
                                : null,
                            item.UnitId,
                            unitLookup.GetValueOrDefault(item.UnitId)
                                ?? string.Empty,
                            item.Quantity,
                            item.UnitPrice,
                            item.TotalPrice
                        )
                    )
                    .ToList()
            );

            return Result.Success(response);
        }
        public async Task<Result> UpdateStatusAsync(int orderId, int currentUserId, OrderStatus newStatus)
        {
            // ========================================
            // Vendor
            // ========================================

            var vendor =
                await _unitOfWork.Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId &&
                            x.IsActive
                    );


            if (vendor is null)
            {
                return Result.Failure(
                    OrderErrors.VendorNotFound
                );
            }


            // ========================================
            // Order Ownership
            // ========================================

            var order =
                await _unitOfWork.Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == orderId &&
                            x.VendorId == vendor.Id
                    );


            if (order is null)
            {
                return Result.Failure(
                    OrderErrors.NotFound
                );
            }


            // idempotent request
            if (order.Status == newStatus)
            {
                return Result.Success();
            }


            // ========================================
            // Validate Transition
            // ========================================

            if (!CanVendorTransition(
                    order.Status,
                    newStatus))
            {
                return Result.Failure(
                    OrderErrors.InvalidStatusTransition
                );
            }


            // ========================================
            // Tracked Order
            // ========================================

            var trackedOrder =
                await _unitOfWork.Orders
                    .FindAsync(order.Id);


            if (trackedOrder is null)
            {
                return Result.Failure(
                    OrderErrors.NotFound
                );
            }


            var oldStatus =
                trackedOrder.Status;

            var now =
                DateTime.UtcNow;


            trackedOrder.Status = newStatus;

            trackedOrder.UpdatedAt = now;


            // ========================================
            // Status Timestamps
            // ========================================

            switch (newStatus)
            {
                case OrderStatus.InProgress:

                    trackedOrder.InProgressAt =
                        now;

                    break;


                case OrderStatus.Shipped:

                    trackedOrder.ShippedAt =
                        now;

                    break;


                case OrderStatus.Delivered:

                    trackedOrder.DeliveredAt =
                        now;

                    break;
            }


            // ========================================
            // History
            // ========================================

            var history =
                new OrderStatusHistory
                {
                    OrderId =
                        trackedOrder.Id,

                    OldStatus =
                        oldStatus,

                    NewStatus =
                        newStatus,

                    ChangedByUserId =
                        currentUserId,

                    ChangedAt =
                        now
                };


            await _unitOfWork
                .OrderStatusHistory
                .InsertAsync(history);


            await _unitOfWork.SaveChangeAsync();


            if (newStatus == OrderStatus.Delivered)
            {
                var settlementResult =
                    await _orderSettlementService
                        .TrySettleAsync(
                            trackedOrder.Id
                        );


                if (settlementResult.IsFailure)
                {
                    return Result.Failure(
                        settlementResult.Error
                    );
                }
            }


            return Result.Success();
        }


        private static bool CanVendorTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return
                currentStatus == OrderStatus.Confirmed &&
                newStatus == OrderStatus.InProgress

                ||

                currentStatus == OrderStatus.InProgress &&
                newStatus == OrderStatus.Shipped

                ||

                currentStatus == OrderStatus.Shipped &&
                newStatus == OrderStatus.Delivered;
        }
    }
}

