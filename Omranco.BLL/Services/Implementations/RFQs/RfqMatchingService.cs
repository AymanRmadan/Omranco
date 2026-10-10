using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.RfqServices;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.RfqServices
{
    public sealed class RfqMatchingService
        : IRfqMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RfqMatchingService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<List<int>>> FindMatchingVendorIdsAsync(int rfqId)
        {

            var rfq =
                await _unitOfWork.Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project,
                        x => x.Categories
                    );


            if (rfq is null)
            {
                return Result.Failure<List<int>>(
                    RfqErrors.NotFound
                );
            }


            // ========================================
            // RFQ Categories
            // ========================================

            var rfqCategoryIds = rfq.Categories
                .Select(x => x.CategoryId)
                .Distinct()
                .ToList();


            if (rfqCategoryIds.Count == 0)
            {
                return Result.Success(
                    new List<int>()
                );
            }


            // ========================================
            // Load Category Tree
            // ========================================

            var categories =
                await _unitOfWork.Categories
                    .GetAllAsync();


            var parentLookup =
                categories.ToDictionary(
                    x => x.Id,
                    x => x.ParentId
                );


            /*
             * لكل Category في الـRFQ
             * هنجيب نفسها وكل الـParents بتوعها.
             *
             * مثال:
             *
             * مواد بناء
             *      ↓
             * حديد
             *      ↓
             * حديد تسليح
             *
             * لو RFQ Category = حديد تسليح
             *
             * Vendor يعتبر Match لو مختار:
             *
             * حديد تسليح
             * أو
             * حديد
             * أو
             * مواد بناء
             */

            var acceptedCategoryIdsByRfqCategory =
                new Dictionary<int, HashSet<int>>();


            foreach (var categoryId in rfqCategoryIds)
            {
                var acceptedIds =
                    new HashSet<int>();

                int? currentId = categoryId;


                while (currentId.HasValue)
                {
                    if (!acceptedIds.Add(
                            currentId.Value))
                    {
                        break;
                    }


                    if (!parentLookup.TryGetValue(
                            currentId.Value,
                            out var parentId))
                    {
                        break;
                    }


                    currentId = parentId;
                }


                acceptedCategoryIdsByRfqCategory[
                    categoryId
                ] = acceptedIds;
            }


            // ========================================
            // Get Eligible Vendors
            // ========================================

            var vendors =
                await _unitOfWork.Vendors
                    .GetAllAsync(
                        filter: x =>
                            x.IsActive &&
                            x.Status ==
                                VendorStatus.Approved &&
                            x.Type ==
                                VendorType.MaterialSupplier,

                        includes:
                        [
                            x => x.ServiceZones,
                            x => x.Categories
                        ]
                    );


            // ========================================
            // Match Zone + Categories
            // ========================================

            var matchedVendorIds =
                vendors
                    .Where(vendor =>
                        vendor.ServiceZones.Any(
                            zone =>
                                zone.ZoneId ==
                                rfq.Project.ZoneId
                        )
                        &&
                        acceptedCategoryIdsByRfqCategory
                            .Values
                            .All(
                                acceptedCategoryIds =>
                                    vendor.Categories
                                        .Where(
                                            category =>
                                                category.IsActive
                                        )
                                        .Any(
                                            vendorCategory =>
                                                acceptedCategoryIds
                                                    .Contains(
                                                        vendorCategory
                                                            .CategoryId
                                                    )
                                        )
                            )
                    )
                    .Select(x => x.Id)
                    .ToList();


            return Result.Success(
                matchedVendorIds
            );
        }
    }
}