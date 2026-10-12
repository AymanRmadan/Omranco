using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Requests;
using Omranco.BLL.DTOS.Vendors.Responses;
using Omranco.BLL.Services.Abstractions.FileServices;
using Omranco.BLL.Services.Abstractions.VendorServices;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.VendorServices
{
    public sealed class VendorService : IVendorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;


        public VendorService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService)
        {
            _unitOfWork =
                unitOfWork;

            _fileStorageService =
                fileStorageService;
        }


        // =====================================================
        // Vendor - Create
        // =====================================================

        public async Task<Result<VendorProfileResponse>> CreateAsync(int currentUserId, CreateVendorProfileRequest request)
        {
            var alreadyExists =
                await _unitOfWork
                    .Vendors
                    .AnyAsync(
                        x =>
                            x.OwnerUserId ==
                            currentUserId
                    );


            if (alreadyExists)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        VendorErrors.AlreadyExists
                    );
            }


            var validation =
                await ValidateProfileAsync(
                    request.BusinessName,
                    request.Address,
                    request.GovernorateId
                );


            if (validation.IsFailure)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        validation.Error
                    );
            }

            var now =
    DateTime.UtcNow;


            var vendor =
                new Vendor
                {
                    OwnerUserId =
                        currentUserId,

                    Type =
                        request.Type,

                    BusinessName =
                        request.BusinessName.Trim(),

                    GovernorateId =
                        request.GovernorateId,

                    Address =
                        request.Address.Trim(),

                    Latitude =
                        request.Latitude,

                    Longitude =
                        request.Longitude,

                    Status =
                        VendorStatus.Draft,

                    IsActive =
                        true,

                    CreatedAt =
                        now,

                    StatusHistory =
                    [
                        new VendorStatusHistory
            {
                OldStatus =
                    null,

                NewStatus =
                    VendorStatus.Draft,

                Reason =
                    null,

                ChangedByUserId =
                    currentUserId,

                ChangedAt =
                    now
            }
                    ]
                };


            await _unitOfWork.Vendors.InsertAsync(vendor);


            await _unitOfWork
                .SaveChangeAsync();


            return await BuildProfileAsync(
                vendor
            );
        }


        // =====================================================
        // Vendor - Get My Profile
        // =====================================================

        public async Task<Result<VendorProfileResponse>>
            GetMyProfileAsync(
                int currentUserId)
        {
            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        VendorErrors.NotFound
                    );
            }


            return await BuildProfileAsync(
                vendor
            );
        }


        // =====================================================
        // Vendor - Update Profile
        // =====================================================

        public async Task<Result<VendorProfileResponse>>
            UpdateAsync(
                int currentUserId,
                UpdateVendorProfileRequest request)
        {
            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        VendorErrors.NotFound
                    );
            }


            if (!CanEditProfile(
                    vendor.Status))
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        VendorErrors.InvalidStatus
                    );
            }


            var validation =
                await ValidateProfileAsync(
                    request.BusinessName,
                    request.Address,
                    request.GovernorateId
                );


            if (validation.IsFailure)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        validation.Error
                    );
            }


            var trackedVendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        vendor.Id
                    );


            if (trackedVendor is null)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        VendorErrors.NotFound
                    );
            }


            trackedVendor.BusinessName =
                request.BusinessName.Trim();

            trackedVendor.GovernorateId =
                request.GovernorateId;

            trackedVendor.Address =
                request.Address.Trim();

            trackedVendor.Latitude =
                request.Latitude;

            trackedVendor.Longitude =
                request.Longitude;

            trackedVendor.UpdatedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return await BuildProfileAsync(
                trackedVendor
            );
        }


        // =====================================================
        // Vendor - Categories
        // =====================================================

        public async Task<Result> SetCategoriesAsync(
            int currentUserId,
            SetVendorCategoriesRequest request)
        {
            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            if (!CanEditProfile(
                    vendor.Status))
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            if (vendor.Type !=
                VendorType.MaterialSupplier)
            {
                return Result.Failure(
                    VendorErrors.CategoriesNotAllowed
                );
            }


            var categoryIds =
                request.CategoryIds
                    .Distinct()
                    .ToList();


            if (categoryIds.Count > 0)
            {
                var categories =
                    await _unitOfWork
                        .Categories
                        .GetAllAsync(
                            filter:
                                x =>
                                    categoryIds.Contains(
                                        x.Id
                                    )
                                    &&
                                    x.IsActive
                        );


                if (categories.Count !=
                    categoryIds.Count)
                {
                    return Result.Failure(
                        VendorErrors.CategoryNotFound
                    );
                }
            }


            var existing =
                await _unitOfWork
                    .VendorCategories
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id
                    );


            foreach (var link in existing)
            {
                var tracked =
                    await _unitOfWork
                        .VendorCategories
                        .FindAsync(
                            link.Id
                        );


                if (tracked is null)
                    continue;


                tracked.IsActive =
                    categoryIds.Contains(
                        link.CategoryId
                    );
            }


            var existingCategoryIds =
                existing
                    .Select(
                        x => x.CategoryId
                    )
                    .ToHashSet();


            var newLinks =
                categoryIds
                    .Where(
                        id =>
                            !existingCategoryIds
                                .Contains(id)
                    )
                    .Select(
                        categoryId =>
                            new VendorCategory
                            {
                                VendorId =
                                    vendor.Id,

                                CategoryId =
                                    categoryId,

                                IsActive =
                                    true,

                                AddedAt =
                                    DateTime.UtcNow
                            }
                    )
                    .ToList();


            if (newLinks.Count > 0)
            {
                await _unitOfWork
                    .VendorCategories
                    .InsertListAsync(
                        newLinks
                    );
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Vendor - Zones
        // =====================================================

        public async Task<Result> SetServiceZonesAsync(
            int currentUserId,
            SetVendorZonesRequest request)
        {
            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            if (!CanEditProfile(
                    vendor.Status))
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var zoneIds =
                request.ZoneIds
                    .Distinct()
                    .ToList();


            if (zoneIds.Count > 0)
            {
                var zones =
                    await _unitOfWork
                        .Zones
                        .GetAllAsync(
                            filter:
                                x =>
                                    zoneIds.Contains(
                                        x.Id
                                    )
                                    &&
                                    x.IsActive
                        );


                if (zones.Count !=
                    zoneIds.Count)
                {
                    return Result.Failure(
                        VendorErrors.ZoneNotFound
                    );
                }
            }


            var existing =
                await _unitOfWork
                    .VendorZones
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id
                    );


            var requestedIds =
                zoneIds.ToHashSet();


            foreach (var zoneLink in existing)
            {
                if (requestedIds.Contains(
                        zoneLink.ZoneId))
                {
                    continue;
                }


                var tracked =
                    await _unitOfWork
                        .VendorZones
                        .FindAsync(
                            zoneLink.Id
                        );


                if (tracked is not null)
                {
                    _unitOfWork
                        .VendorZones
                        .Delete(
                            tracked
                        );
                }
            }


            var existingZoneIds =
                existing
                    .Select(
                        x => x.ZoneId
                    )
                    .ToHashSet();


            var newZones =
                zoneIds
                    .Where(
                        id =>
                            !existingZoneIds
                                .Contains(id)
                    )
                    .Select(
                        zoneId =>
                            new VendorZone
                            {
                                VendorId =
                                    vendor.Id,

                                ZoneId =
                                    zoneId
                            }
                    )
                    .ToList();


            if (newZones.Count > 0)
            {
                await _unitOfWork
                    .VendorZones
                    .InsertListAsync(
                        newZones
                    );
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Vendor - Requirements
        // =====================================================

        public async Task<
            Result<List<VendorDocumentRequirementResponse>>>
            GetDocumentRequirementsAsync(
                int currentUserId)
        {
            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<List<VendorDocumentRequirementResponse>>(
                        VendorErrors.NotFound
                    );
            }


            var requirements =
                await _unitOfWork
                    .VendorDocumentRequirements
                    .GetAllAsync(
                        filter:
                            x =>
                                x.IsActive
                                &&
                                (
                                    !x.VendorType.HasValue
                                    ||
                                    x.VendorType.Value ==
                                        vendor.Type
                                ),

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.SortOrder
                                )
                    );


            var documents =
                await _unitOfWork
                    .VendorDocuments
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id
                    );


            var responses =
                requirements
                    .Select(
                        requirement =>
                        {
                            var document =
                                documents
                                    .FirstOrDefault(
                                        x =>
                                            x.RequirementId ==
                                            requirement.Id
                                    );


                            return new VendorDocumentRequirementResponse(
                                requirement.Id,
                                requirement.Name,
                                requirement.VendorType,
                                requirement.IsRequired,
                                requirement.SortOrder,

                                document is null
                                    ? null
                                    : MapDocument(
                                        document,
                                        requirement
                                    )
                            );
                        }
                    )
                    .ToList();


            return Result.Success(
                responses
            );
        }


        // =====================================================
        // Vendor - Upload Document
        // =====================================================

        public async Task<Result<VendorDocumentResponse>>
            UploadDocumentAsync(
                int requirementId,
                int currentUserId,
                UploadVendorDocumentRequest request)
        {
            const long maxSize =
                10 * 1024 * 1024;


            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorDocumentResponse>(
                        VendorErrors.NotFound
                    );
            }


            /*
             * Pending is allowed here because an Admin
             * may reject one document while reviewing.
             */
            if (
                vendor.Status !=
                    VendorStatus.Draft
                &&
                vendor.Status !=
                    VendorStatus.Rejected
                &&
                vendor.Status !=
                    VendorStatus.Pending
            )
            {
                return Result
                    .Failure<VendorDocumentResponse>(
                        VendorErrors.InvalidStatus
                    );
            }


            var requirement =
                await _unitOfWork
                    .VendorDocumentRequirements
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                                requirementId
                            &&
                            x.IsActive
                            &&
                            (
                                !x.VendorType.HasValue
                                ||
                                x.VendorType.Value ==
                                    vendor.Type
                            )
                    );


            if (requirement is null)
            {
                return Result
                    .Failure<VendorDocumentResponse>(
                        VendorErrors
                            .DocumentRequirementNotFound
                    );
            }


            if (
                request.Length <= 0
                ||
                request.Length > maxSize
            )
            {
                return Result
                    .Failure<VendorDocumentResponse>(
                        VendorErrors.FileTooLarge
                    );
            }


            var extension =
                Path.GetExtension(
                    request.FileName
                )
                .ToLowerInvariant();


            var allowedExtensions =
                new[]
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };


            if (!allowedExtensions.Contains(
                    extension))
            {
                return Result
                    .Failure<VendorDocumentResponse>(
                        VendorErrors.InvalidFile
                    );
            }


            var newPath =
                await _fileStorageService
                    .SaveAsync(
                        request.Content,
                        request.FileName,
                        $"uploads/vendors/{vendor.Id}/documents"
                    );


            var existing =
                await _unitOfWork
                    .VendorDocuments
                    .FirstOrDefaultAsync(
                        x =>
                            x.VendorId ==
                                vendor.Id
                            &&
                            x.RequirementId ==
                                requirement.Id
                    );


            string? oldPath =
                null;

            VendorDocument document;


            if (existing is null)
            {
                document =
                    new VendorDocument
                    {
                        VendorId =
                            vendor.Id,

                        RequirementId =
                            requirement.Id,

                        FilePath =
                            newPath,

                        ReviewStatus =
                            DocumentReviewStatus.Pending,

                        UploadedAt =
                            DateTime.UtcNow
                    };


                await _unitOfWork
                    .VendorDocuments
                    .InsertAsync(
                        document
                    );
            }
            else
            {
                var tracked =
                    await _unitOfWork
                        .VendorDocuments
                        .FindAsync(
                            existing.Id
                        );


                if (tracked is null)
                {
                    return Result
                        .Failure<VendorDocumentResponse>(
                            VendorErrors.DocumentNotFound
                        );
                }


                oldPath =
                    tracked.FilePath;


                tracked.FilePath =
                    newPath;

                tracked.ReviewStatus =
                    DocumentReviewStatus.Pending;

                tracked.ReviewNotes =
                    null;

                tracked.ReviewedByUserId =
                    null;

                tracked.ReviewedAt =
                    null;

                tracked.UploadedAt =
                    DateTime.UtcNow;


                document =
                    tracked;
            }


            await _unitOfWork
                .SaveChangeAsync();


            if (!string.IsNullOrWhiteSpace(
                    oldPath))
            {
                await _fileStorageService
                    .DeleteAsync(
                        oldPath
                    );
            }


            return Result.Success(
                MapDocument(
                    document,
                    requirement
                )
            );
        }


        // =====================================================
        // Vendor - Submit For Review
        // =====================================================

        public async Task<Result> SubmitForReviewAsync(int currentUserId)
        {
            var vendor =
                await GetMyVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            /*
             * Idempotent submit.
             */
            if (vendor.Status ==
                VendorStatus.Pending)
            {
                return Result.Success();
            }


            if (
                vendor.Status !=
                    VendorStatus.Draft
                &&
                vendor.Status !=
                    VendorStatus.Rejected
            )
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var validation =
                await ValidateVendorReadyAsync(
                    vendor,
                    requireApprovedDocuments:
                        false
                );


            if (validation.IsFailure)
            {
                return validation;
            }


            var trackedVendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        vendor.Id
                    );


            if (trackedVendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            var oldStatus =
    trackedVendor.Status;

            var now =
                DateTime.UtcNow;


            trackedVendor.Status =
                VendorStatus.Pending;

            trackedVendor.StatusReason =
                null;

            trackedVendor.SubmittedForReviewAt =
                now;

            trackedVendor.UpdatedAt =
                now;

            await AddStatusHistoryAsync(trackedVendor.Id, oldStatus, VendorStatus.Pending, currentUserId);
            await _unitOfWork.SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Vendor List
        // =====================================================

        public async Task<
            Result<List<AdminVendorListItemResponse>>>
            GetAllAsync(
                VendorStatus? status)
        {
            var vendors =
                await _unitOfWork
                    .Vendors
                    .GetAllAsync(
                        filter:
                            status.HasValue
                                ? x =>
                                    x.Status ==
                                    status.Value
                                : null,

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.CreatedAt
                                ),

                        includes:
                        [
                            x => x.Governorate
                        ]
                    );


            return Result.Success(
                vendors
                    .Select(
                        x =>
                            new AdminVendorListItemResponse(
                                x.Id,
                                x.OwnerUserId,
                                x.Type,
                                x.BusinessName,
                                x.Governorate.NameAr,
                                x.Status,
                                x.IsActive,
                                x.CreatedAt,
                                x.SubmittedForReviewAt
                            )
                    )
                    .ToList()
            );
        }


        // =====================================================
        // Admin - Vendor Details
        // =====================================================

        public async Task<Result<VendorProfileResponse>>
            GetByIdAsync(
                int vendorId)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        vendorId
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorProfileResponse>(
                        VendorErrors.NotFound
                    );
            }


            return await BuildProfileAsync(
                vendor
            );
        }


        // =====================================================
        // Admin - Approve Vendor
        // =====================================================

        public async Task<Result> ApproveAsync(int vendorId, int currentUserId)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        vendorId
                    );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            if (vendor.Status !=
                VendorStatus.Pending)
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var validation =
                await ValidateVendorReadyAsync(
                    vendor,
                    requireApprovedDocuments:
                        true
                );


            if (validation.IsFailure)
            {
                return validation;
            }


            var trackedVendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        vendor.Id
                    );


            if (trackedVendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            var oldStatus = trackedVendor.Status;

            var now =
                DateTime.UtcNow;


            trackedVendor.Status =
                VendorStatus.Approved;

            trackedVendor.StatusReason =
                null;

            trackedVendor.ApprovedAt =
                now;

            trackedVendor.IsActive =
                true;

            trackedVendor.UpdatedAt =
                now;


            await AddStatusHistoryAsync(
                trackedVendor.Id,
                oldStatus,
                VendorStatus.Approved,
                currentUserId
            );


            await _unitOfWork
                .SaveChangeAsync();

            return Result.Success();
        }


        // =====================================================
        // Admin - Reject Vendor
        // =====================================================

        public async Task<Result> RejectAsync(int vendorId, int currentUserId, RejectVendorRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Reason))
            {
                return Result.Failure(
                    VendorErrors.ReasonRequired
                );
            }


            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        vendorId
                    );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            if (vendor.Status !=
                VendorStatus.Pending)
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var trackedVendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        vendor.Id
                    );


            if (trackedVendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }

            var oldStatus =
                trackedVendor.Status;

            var now =
                DateTime.UtcNow;

            var reason =
                request.Reason.Trim();


            trackedVendor.Status =
                VendorStatus.Rejected;

            trackedVendor.StatusReason =
                reason;

            trackedVendor.UpdatedAt =
                now;


            await AddStatusHistoryAsync(
                trackedVendor.Id,
                oldStatus,
                VendorStatus.Rejected,
                currentUserId,
                reason
            );


            await _unitOfWork.SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Suspend
        // =====================================================

        public async Task<Result> SuspendAsync(int vendorId, int currentUserId, SuspendVendorRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.Reason))
            {
                return Result.Failure(
                    VendorErrors.ReasonRequired
                );
            }


            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        vendorId
                    );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            if (vendor.Status !=
                VendorStatus.Approved)
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var trackedVendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        vendor.Id
                    );


            if (trackedVendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            var oldStatus =
    trackedVendor.Status;

            var now =
                DateTime.UtcNow;

            var reason =
                request.Reason.Trim();


            trackedVendor.Status =
                VendorStatus.Suspended;

            trackedVendor.StatusReason =
                reason;

            trackedVendor.IsActive =
                false;

            trackedVendor.UpdatedAt =
                now;


            await AddStatusHistoryAsync(
                trackedVendor.Id,
                oldStatus,
                VendorStatus.Suspended,
                currentUserId,
                reason
            );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Reactivate
        // =====================================================

        public async Task<Result> ReactivateAsync(int vendorId, int currentUserId)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        vendorId
                    );


            if (vendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            if (vendor.Status !=
                VendorStatus.Suspended)
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var trackedVendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        vendor.Id
                    );


            if (trackedVendor is null)
            {
                return Result.Failure(
                    VendorErrors.NotFound
                );
            }


            var oldStatus =
    trackedVendor.Status;

            var now =
                DateTime.UtcNow;


            trackedVendor.Status =
                VendorStatus.Approved;

            trackedVendor.StatusReason =
                null;

            trackedVendor.IsActive =
                true;

            trackedVendor.ApprovedAt ??=
                now;

            trackedVendor.UpdatedAt =
                now;


            await AddStatusHistoryAsync(
                trackedVendor.Id,
                oldStatus,
                VendorStatus.Approved,
                currentUserId
            );


            await _unitOfWork
                .SaveChangeAsync();
            return Result.Success();
        }


        // =====================================================
        // Admin - Approve Document
        // =====================================================

        public async Task<Result> ApproveDocumentAsync(
            int documentId,
            int currentUserId)
        {
            var document =
                await _unitOfWork
                    .VendorDocuments
                    .GetByIdAsync(
                        documentId
                    );


            if (document is null)
            {
                return Result.Failure(
                    VendorErrors.DocumentNotFound
                );
            }


            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        document.VendorId
                    );


            if (
                vendor is null
                ||
                vendor.Status !=
                    VendorStatus.Pending
            )
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var trackedDocument =
                await _unitOfWork
                    .VendorDocuments
                    .FindAsync(
                        document.Id
                    );


            if (trackedDocument is null)
            {
                return Result.Failure(
                    VendorErrors.DocumentNotFound
                );
            }


            trackedDocument.ReviewStatus =
                DocumentReviewStatus.Approved;

            trackedDocument.ReviewNotes =
                null;

            trackedDocument.ReviewedByUserId =
                currentUserId;

            trackedDocument.ReviewedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Reject Document
        // =====================================================

        public async Task<Result> RejectDocumentAsync(
            int documentId,
            int currentUserId,
            RejectVendorDocumentRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.ReviewNotes))
            {
                return Result.Failure(
                    VendorErrors.ReviewNotesRequired
                );
            }


            var document =
                await _unitOfWork
                    .VendorDocuments
                    .GetByIdAsync(
                        documentId
                    );


            if (document is null)
            {
                return Result.Failure(
                    VendorErrors.DocumentNotFound
                );
            }


            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        document.VendorId
                    );


            if (
                vendor is null
                ||
                vendor.Status !=
                    VendorStatus.Pending
            )
            {
                return Result.Failure(
                    VendorErrors.InvalidStatus
                );
            }


            var trackedDocument =
                await _unitOfWork
                    .VendorDocuments
                    .FindAsync(
                        document.Id
                    );


            if (trackedDocument is null)
            {
                return Result.Failure(
                    VendorErrors.DocumentNotFound
                );
            }


            trackedDocument.ReviewStatus =
                DocumentReviewStatus.Rejected;

            trackedDocument.ReviewNotes =
                request.ReviewNotes.Trim();

            trackedDocument.ReviewedByUserId =
                currentUserId;

            trackedDocument.ReviewedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Helpers
        // =====================================================

        private async Task<Vendor?> GetMyVendorAsync(
            int currentUserId)
        {
            return await _unitOfWork
                .Vendors
                .FirstOrDefaultAsync(
                    x =>
                        x.OwnerUserId ==
                        currentUserId
                );
        }


        private async Task<Result> ValidateProfileAsync(
            string businessName,
            string address,
            int governorateId)
        {
            if (string.IsNullOrWhiteSpace(
                    businessName))
            {
                return Result.Failure(
                    VendorErrors.InvalidBusinessName
                );
            }


            if (string.IsNullOrWhiteSpace(
                    address))
            {
                return Result.Failure(
                    VendorErrors.InvalidAddress
                );
            }


            var governorateExists =
                await _unitOfWork
                    .Governorates
                    .AnyAsync(
                        x =>
                            x.Id ==
                                governorateId
                            &&
                            x.IsActive
                    );


            if (!governorateExists)
            {
                return Result.Failure(
                    VendorErrors.GovernorateNotFound
                );
            }


            return Result.Success();
        }


        private async Task<Result>
            ValidateVendorReadyAsync(
                Vendor vendor,
                bool requireApprovedDocuments)
        {
            var profileValidation =
                await ValidateProfileAsync(
                    vendor.BusinessName,
                    vendor.Address,
                    vendor.GovernorateId
                );


            if (profileValidation.IsFailure)
            {
                return profileValidation;
            }


            // ==========================================
            // Zones required
            // ==========================================

            var hasZone =
                await _unitOfWork
                    .VendorZones
                    .AnyAsync(
                        x =>
                            x.VendorId ==
                            vendor.Id
                    );


            if (!hasZone)
            {
                return Result.Failure(
                    VendorErrors
                        .ServiceZonesRequired
                );
            }


            // ==========================================
            // Material categories required
            // ==========================================

            if (vendor.Type ==
                VendorType.MaterialSupplier)
            {
                var hasCategory =
                    await _unitOfWork
                        .VendorCategories
                        .AnyAsync(
                            x =>
                                x.VendorId ==
                                    vendor.Id
                                &&
                                x.IsActive
                        );


                if (!hasCategory)
                {
                    return Result.Failure(
                        VendorErrors
                            .CategoriesRequired
                    );
                }
            }


            // ==========================================
            // Required documents
            // ==========================================

            var requirements =
                await _unitOfWork
                    .VendorDocumentRequirements
                    .GetAllAsync(
                        filter:
                            x =>
                                x.IsActive
                                &&
                                x.IsRequired
                                &&
                                (
                                    !x.VendorType.HasValue
                                    ||
                                    x.VendorType.Value ==
                                        vendor.Type
                                )
                    );


            if (requirements.Count == 0)
            {
                return Result.Success();
            }


            var documents =
                await _unitOfWork
                    .VendorDocuments
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id
                    );


            foreach (var requirement in requirements)
            {
                var document =
                    documents
                        .FirstOrDefault(
                            x =>
                                x.RequirementId ==
                                requirement.Id
                        );


                if (document is null)
                {
                    return Result.Failure(
                        VendorErrors
                            .MissingRequiredDocuments
                    );
                }


                if (
                    !requireApprovedDocuments
                    &&
                    document.ReviewStatus ==
                        DocumentReviewStatus.Rejected
                )
                {
                    return Result.Failure(
                        VendorErrors
                            .RejectedRequiredDocuments
                    );
                }


                if (
                    requireApprovedDocuments
                    &&
                    document.ReviewStatus !=
                        DocumentReviewStatus.Approved
                )
                {
                    return Result.Failure(
                        VendorErrors
                            .RequiredDocumentsNotApproved
                    );
                }
            }


            return Result.Success();
        }


        private async Task<Result<VendorProfileResponse>>
            BuildProfileAsync(
                Vendor vendor)
        {
            var governorate =
                await _unitOfWork
                    .Governorates
                    .GetByIdAsync(
                        vendor.GovernorateId
                    );


            var categoryLinks =
                await _unitOfWork
                    .VendorCategories
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                    vendor.Id
                                &&
                                x.IsActive
                    );


            var categoryIds =
                categoryLinks
                    .Select(
                        x => x.CategoryId
                    )
                    .ToList();


            var categories =
                categoryIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .Categories
                        .GetAllAsync(
                            filter:
                                x =>
                                    categoryIds
                                        .Contains(
                                            x.Id
                                        )
                        );


            var zoneLinks =
                await _unitOfWork
                    .VendorZones
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id
                    );


            var zoneIds =
                zoneLinks
                    .Select(
                        x => x.ZoneId
                    )
                    .ToList();


            var zones =
                zoneIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .Zones
                        .GetAllAsync(
                            filter:
                                x =>
                                    zoneIds.Contains(
                                        x.Id
                                    )
                        );


            var documents =
                await _unitOfWork
                    .VendorDocuments
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id
                    );


            var requirementIds =
                documents
                    .Select(
                        x => x.RequirementId
                    )
                    .Distinct()
                    .ToList();


            var requirements =
                requirementIds.Count == 0
                    ? []
                    : await _unitOfWork
                        .VendorDocumentRequirements
                        .GetAllAsync(
                            filter:
                                x =>
                                    requirementIds
                                        .Contains(
                                            x.Id
                                        )
                        );


            var categoryLookup =
                categories.ToDictionary(
                    x => x.Id,
                    x => x.Name
                );


            var zoneLookup =
                zones.ToDictionary(
                    x => x.Id
                );


            var requirementLookup =
                requirements.ToDictionary(
                    x => x.Id
                );


            var response =
                new VendorProfileResponse(
                    vendor.Id,
                    vendor.OwnerUserId,
                    vendor.Type,
                    vendor.BusinessName,
                    vendor.GovernorateId,
                    governorate?.NameAr
                        ?? string.Empty,
                    vendor.Address,
                    vendor.Latitude,
                    vendor.Longitude,
                    vendor.Status,
                    vendor.StatusReason,
                    vendor.RatingAverage,
                    vendor.RatingsCount,
                    vendor.SubmittedForReviewAt,
                    vendor.ApprovedAt,
                    vendor.IsActive,
                    vendor.CreatedAt,
                    vendor.UpdatedAt,

                    categoryLinks
                        .Where(
                            x =>
                                categoryLookup
                                    .ContainsKey(
                                        x.CategoryId
                                    )
                        )
                        .Select(
                            x =>
                                new VendorCategoryResponse(
                                    x.CategoryId,
                                    categoryLookup[
                                        x.CategoryId
                                    ]
                                )
                        )
                        .ToList(),

                    zoneLinks
                        .Where(
                            x =>
                                zoneLookup
                                    .ContainsKey(
                                        x.ZoneId
                                    )
                        )
                        .Select(
                            x =>
                            {
                                var zone =
                                    zoneLookup[
                                        x.ZoneId
                                    ];


                                return new VendorZoneResponse(
                                    zone.Id,
                                    zone.NameAr,
                                    zone.NameEn
                                );
                            }
                        )
                        .ToList(),

                    documents
                        .Where(
                            x =>
                                requirementLookup
                                    .ContainsKey(
                                        x.RequirementId
                                    )
                        )
                        .Select(
                            x =>
                                MapDocument(
                                    x,
                                    requirementLookup[
                                        x.RequirementId
                                    ]
                                )
                        )
                        .ToList()
                );


            return Result.Success(
                response
            );
        }


        private static VendorDocumentResponse MapDocument(
            VendorDocument document,
            VendorDocumentRequirement requirement)
        {
            return new VendorDocumentResponse(
                document.Id,
                document.RequirementId,
                requirement.Name,
                requirement.IsRequired,
                document.FilePath,
                document.ReviewStatus,
                document.ReviewNotes,
                document.UploadedAt,
                document.ReviewedAt
            );
        }


        private static bool CanEditProfile(VendorStatus status)
        {
            return
                status ==
                    VendorStatus.Draft
                ||
                status ==
                    VendorStatus.Rejected;
        }

        private async Task AddStatusHistoryAsync(int vendorId, VendorStatus oldStatus, VendorStatus newStatus, int? changedByUserId, string? reason = null)
        {
            var history =
                new VendorStatusHistory
                {
                    VendorId =
                        vendorId,

                    OldStatus =
                        oldStatus,

                    NewStatus =
                        newStatus,

                    Reason =
                        string.IsNullOrWhiteSpace(reason)
                            ? null
                            : reason.Trim(),

                    ChangedByUserId =
                        changedByUserId,

                    ChangedAt =
                        DateTime.UtcNow
                };


            await _unitOfWork
                .VendorStatusHistorys
                .InsertAsync(
                    history
                );
        }
    }


}