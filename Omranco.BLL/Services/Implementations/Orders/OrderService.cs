using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Responses;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.OrderServices
{
    public sealed class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderSettlementService _orderSettlementService;


        public OrderService(
            IUnitOfWork unitOfWork,
            IOrderSettlementService orderSettlementService)
        {
            _unitOfWork =
                unitOfWork;

            _orderSettlementService =
                orderSettlementService;
        }


        // =====================================================
        // Vendor - Orders List
        // =====================================================

        public async Task<Result<List<VendorOrderSummaryResponse>>>
            GetVendorOrdersAsync(
                int currentUserId,
                OrderStatus? status)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<List<VendorOrderSummaryResponse>>(
                        OrderErrors.VendorNotFound
                    );
            }


            var orders =
                await _unitOfWork
                    .Orders
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                    vendor.Id
                                &&
                                (
                                    !status.HasValue
                                    ||
                                    x.Status ==
                                        status.Value
                                ),

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.CreatedAt
                                ),

                        includes:
                        [
                            x => x.Project
                        ]
                    );


            var response =
                orders
                    .Select(
                        order =>
                            new VendorOrderSummaryResponse(
                                order.Id,
                                order.Number,
                                order.ProjectId,
                                order.Project.Name,
                                order.Status,
                                order.TotalAmount,
                                order.CreatedAt,
                                order.DeliveredAt
                            )
                    )
                    .ToList();


            return Result.Success(
                response
            );
        }


        // =====================================================
        // Vendor - Order Details
        // =====================================================

        public async Task<Result<VendorOrderDetailsResponse>>
            GetVendorOrderDetailsAsync(
                int orderId,
                int currentUserId)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorOrderDetailsResponse>(
                        OrderErrors.VendorNotFound
                    );
            }


            var order =
                await _unitOfWork
                    .Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                                orderId
                            &&
                            x.VendorId ==
                                vendor.Id,

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


            var catalog =
                await LoadCatalogDataAsync(
                    order.Items
                );


            var response =
                new VendorOrderDetailsResponse(
                    order.Id,
                    order.Number,
                    order.Status.ToString(),
                    order.Project.Name,
                    order.Project.Address,
                    order.AcceptedByAccountUser
                        .User.Name,
                    order.AcceptedByAccountUser
                        .User.PhoneNumber,
                    order.Subtotal,
                    order.ShippingCost,
                    order.TotalAmount,
                    order.CreatedAt,
                    order.DeliveredAt,

                    order.Items
                        .Select(
                            item =>
                                new VendorOrderItemResponse(
                                    item.ProductId,

                                    catalog.ProductNames
                                        .GetValueOrDefault(
                                            item.ProductId
                                        )
                                    ?? string.Empty,

                                    item.ProductVariantId,

                                    item.ProductVariantId
                                        .HasValue
                                            ? catalog
                                                .VariantNames
                                                .GetValueOrDefault(
                                                    item
                                                        .ProductVariantId
                                                        .Value
                                                )
                                            : null,

                                    item.UnitId,

                                    catalog.UnitNames
                                        .GetValueOrDefault(
                                            item.UnitId
                                        )
                                    ?? string.Empty,

                                    item.Quantity,
                                    item.UnitPrice,
                                    item.TotalPrice
                                )
                        )
                        .ToList()
                );


            return Result.Success(
                response
            );
        }


        // =====================================================
        // Vendor - Update Status
        // =====================================================

        public async Task<Result> UpdateStatusAsync(
            int orderId,
            int currentUserId,
            OrderStatus newStatus)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (vendor is null)
            {
                return Result.Failure(
                    OrderErrors.VendorNotFound
                );
            }


            var order =
                await _unitOfWork
                    .Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                                orderId
                            &&
                            x.VendorId ==
                                vendor.Id
                    );


            if (order is null)
            {
                return Result.Failure(
                    OrderErrors.NotFound
                );
            }


            // ==========================================
            // Idempotency
            // ==========================================

            if (order.Status ==
                newStatus)
            {
                /*
                 * لو Delivered واتكرر الطلب،
                 * نعمل settlement reconciliation
                 * بدل ما نرجع فورًا.
                 */
                if (newStatus ==
                    OrderStatus.Delivered)
                {
                    var settlementResult =
                        await _orderSettlementService
                            .TrySettleAsync(
                                order.Id
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


            // ==========================================
            // Validate transition
            // ==========================================

            if (!CanVendorTransition(
                    order.Status,
                    newStatus))
            {
                return Result.Failure(
                    OrderErrors
                        .InvalidStatusTransition
                );
            }


            var trackedOrder =
                await _unitOfWork
                    .Orders
                    .FindAsync(
                        order.Id
                    );


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


            trackedOrder.Status =
                newStatus;

            trackedOrder.UpdatedAt =
                now;


            // ==========================================
            // Status timestamps
            // ==========================================

            switch (newStatus)
            {
                case OrderStatus.InProgress:

                    trackedOrder.InProgressAt ??=
                        now;

                    break;


                case OrderStatus.Shipped:

                    trackedOrder.ShippedAt ??=
                        now;

                    break;


                case OrderStatus.Delivered:

                    trackedOrder.DeliveredAt ??=
                        now;

                    break;
            }


            // ==========================================
            // History
            // ==========================================

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
                .InsertAsync(
                    history
                );


            await _unitOfWork
                .SaveChangeAsync();


            // ==========================================
            // Settlement after Delivery
            // ==========================================

            if (newStatus ==
                OrderStatus.Delivered)
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


        // =====================================================
        // Customer - Orders List
        // =====================================================

        public async Task<Result<List<CustomerOrderSummaryResponse>>>
            GetCustomerOrdersAsync(
                int customerAccountId,
                int currentUserId,
                OrderStatus? status)
        {
            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.CustomerAccountId ==
                                customerAccountId
                            &&
                            x.UserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return Result
                    .Failure<List<CustomerOrderSummaryResponse>>(
                        OrderErrors
                            .CustomerAccountNotFound
                    );
            }


            List<int> accessibleProjectIds;


            // ==========================================
            // Owner sees all projects
            // ==========================================

            if (accountUser.Role ==
                CustomerAccountUserRole.Owner)
            {
                var projects =
                    await _unitOfWork
                        .Projects
                        .GetAllAsync(
                            filter:
                                x =>
                                    x.CustomerAccountId ==
                                    customerAccountId
                        );


                accessibleProjectIds =
                    projects
                        .Select(
                            x => x.Id
                        )
                        .ToList();
            }


            // ==========================================
            // Member sees assigned projects only
            // ==========================================

            else
            {
                var memberships =
                    await _unitOfWork
                        .ProjectMembers
                        .GetAllAsync(
                            filter:
                                x =>
                                    x.CustomerAccountUserId ==
                                        accountUser.Id
                                    &&
                                    x.IsActive
                        );


                accessibleProjectIds =
                    memberships
                        .Select(
                            x => x.ProjectId
                        )
                        .Distinct()
                        .ToList();
            }


            if (accessibleProjectIds.Count == 0)
            {
                return Result.Success(
                    new List<CustomerOrderSummaryResponse>()
                );
            }


            var orders =
                await _unitOfWork
                    .Orders
                    .GetAllAsync(
                        filter:
                            x =>
                                accessibleProjectIds
                                    .Contains(
                                        x.ProjectId
                                    )
                                &&
                                (
                                    !status.HasValue
                                    ||
                                    x.Status ==
                                        status.Value
                                ),

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.CreatedAt
                                ),

                        includes:
                        [
                            x => x.Project,
                            x => x.Vendor
                        ]
                    );


            var response =
                orders
                    .Select(
                        order =>
                            new CustomerOrderSummaryResponse(
                                order.Id,
                                order.Number,
                                order.ProjectId,
                                order.Project.Name,
                                order.VendorId,
                                order.Vendor.BusinessName,
                                order.Status,
                                order.TotalAmount,
                                order.CreatedAt,
                                order.DeliveredAt
                            )
                    )
                    .ToList();


            return Result.Success(
                response
            );
        }


        // =====================================================
        // Customer - Order Details
        // =====================================================

        public async Task<Result<CustomerOrderDetailsResponse>>
            GetCustomerOrderDetailsAsync(
                int orderId,
                int currentUserId)
        {
            var order =
                await _unitOfWork
                    .Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            orderId,

                        x => x.Project,
                        x => x.Vendor,
                        x => x.Items,
                        x => x.StatusHistory,
                        x => x.Payment
                    );


            if (order is null)
            {
                return Result
                    .Failure<CustomerOrderDetailsResponse>(
                        OrderErrors.NotFound
                    );
            }


            var hasAccess =
                await HasCustomerAccessAsync(
                    order.ProjectId,
                    order.Project.CustomerAccountId,
                    currentUserId
                );


            if (!hasAccess)
            {
                return Result
                    .Failure<CustomerOrderDetailsResponse>(
                        OrderErrors.AccessDenied
                    );
            }


            var catalog =
                await LoadCatalogDataAsync(
                    order.Items
                );


            var response =
                new CustomerOrderDetailsResponse(
                    order.Id,
                    order.Number,
                    order.RfqId,
                    order.BidId,
                    order.ProjectId,
                    order.Project.Name,
                    order.Project.Address,
                    order.VendorId,
                    order.Vendor.BusinessName,
                    order.Status,
                    order.Payment?.Status,
                    order.Subtotal,
                    order.ShippingCost,
                    order.TotalAmount,
                    order.CreatedAt,
                    order.InProgressAt,
                    order.ShippedAt,
                    order.DeliveredAt,
                    order.CancelledAt,

                    order.Items
                        .Select(
                            item =>
                                new CustomerOrderItemResponse(
                                    item.ProductId,

                                    catalog.ProductNames
                                        .GetValueOrDefault(
                                            item.ProductId
                                        )
                                    ?? string.Empty,

                                    item.ProductVariantId,

                                    item.ProductVariantId
                                        .HasValue
                                            ? catalog
                                                .VariantNames
                                                .GetValueOrDefault(
                                                    item
                                                        .ProductVariantId
                                                        .Value
                                                )
                                            : null,

                                    item.UnitId,

                                    catalog.UnitNames
                                        .GetValueOrDefault(
                                            item.UnitId
                                        )
                                    ?? string.Empty,

                                    item.Quantity,
                                    item.UnitPrice,
                                    item.TotalPrice
                                )
                        )
                        .ToList(),

                    order.StatusHistory
                        .OrderBy(
                            x => x.ChangedAt
                        )
                        .Select(
                            history =>
                                new OrderStatusHistoryResponse(
                                    history.Id,
                                    history.OldStatus,
                                    history.NewStatus,
                                    history.ChangedByUserId,
                                    history.ChangedAt
                                )
                        )
                        .ToList()
                );


            return Result.Success(
                response
            );
        }


        // =====================================================
        // Helpers
        // =====================================================

        private async Task<bool>
            HasCustomerAccessAsync(
                int projectId,
                int customerAccountId,
                int currentUserId)
        {
            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.CustomerAccountId ==
                                customerAccountId
                            &&
                            x.UserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return false;
            }


            if (accountUser.Role ==
                CustomerAccountUserRole.Owner)
            {
                return true;
            }


            return await _unitOfWork
                .ProjectMembers
                .AnyAsync(
                    x =>
                        x.ProjectId ==
                            projectId
                        &&
                        x.CustomerAccountUserId ==
                            accountUser.Id
                        &&
                        x.IsActive
                );
        }


        private async Task<CatalogLookupData>
            LoadCatalogDataAsync(
                IEnumerable<OrderItem> items)
        {
            var itemList =
                items.ToList();


            var productIds =
                itemList
                    .Select(
                        x => x.ProductId
                    )
                    .Distinct()
                    .ToList();


            var unitIds =
                itemList
                    .Select(
                        x => x.UnitId
                    )
                    .Distinct()
                    .ToList();


            var variantIds =
                itemList
                    .Where(
                        x =>
                            x.ProductVariantId
                                .HasValue
                    )
                    .Select(
                        x =>
                            x.ProductVariantId!
                                .Value
                    )
                    .Distinct()
                    .ToList();


            var products =
                productIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .Products
                        .GetAllAsync(
                            x =>
                                productIds.Contains(
                                    x.Id
                                )
                        );


            var units =
                unitIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .Units
                        .GetAllAsync(
                            x =>
                                unitIds.Contains(
                                    x.Id
                                )
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


            return new CatalogLookupData(
                products.ToDictionary(
                    x => x.Id,
                    x => x.Name
                ),

                units.ToDictionary(
                    x => x.Id,
                    x => x.Name
                ),

                variants.ToDictionary(
                    x => x.Id,
                    x => x.Name
                )
            );
        }


        private static bool CanVendorTransition(
            OrderStatus currentStatus,
            OrderStatus newStatus)
        {
            return
                (
                    currentStatus ==
                        OrderStatus.Confirmed
                    &&
                    newStatus ==
                        OrderStatus.InProgress
                )
                ||
                (
                    currentStatus ==
                        OrderStatus.InProgress
                    &&
                    newStatus ==
                        OrderStatus.Shipped
                )
                ||
                (
                    currentStatus ==
                        OrderStatus.Shipped
                    &&
                    newStatus ==
                        OrderStatus.Delivered
                );
        }


        private sealed record CatalogLookupData(
            Dictionary<int, string> ProductNames,
            Dictionary<int, string> UnitNames,
            Dictionary<int, string> VariantNames
        );
    }
}