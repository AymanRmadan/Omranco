using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Rfqs;
using Omranco.BLL.Services.Abstractions.FileServices;
using Omranco.BLL.Services.Abstractions.RfqServices;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Rfqs;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.RfqServices
{
    public sealed class RfqService
        : IRfqService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRfqMatchingService _matchingService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IRfqMatchingService _rfqMatchingService;



        public RfqService(
            IUnitOfWork unitOfWork,
            IRfqMatchingService matchingService,
            IFileStorageService fileStorageService,
            IRfqMatchingService rfqMatchingService)
        {
            _unitOfWork =
                unitOfWork;

            _matchingService =
                matchingService;

            _fileStorageService =
                fileStorageService;
            _rfqMatchingService = rfqMatchingService;
        }

        public async Task<Result<int>> CreateDraftAsync(int projectId, int currentUserId, CreateRfqRequest request)
        {
            var project =
                await _unitOfWork
                    .Projects
                    .GetByIdAsync(
                        projectId
                    );


            if (project is null
                ||
                !project.IsActive)
            {
                return Result.Failure<int>(
                    RfqErrors.ProjectNotFound
                );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result.Failure<int>(
                    RfqErrors.AccessDenied
                );
            }


            var validation =
                await ValidateDraftDataAsync(
                    request.DeadlineAt,
                    request.DesiredDeliveryDate,
                    request.CategoryIds,
                    request.Items
                );


            if (validation.IsFailure)
            {
                return Result.Failure<int>(
                    validation.Error
                );
            }


            var now =
                DateTime.UtcNow;


            var rfq =
                new Rfq
                {
                    Number =
                        GenerateRfqNumber(),

                    ProjectId =
                        project.Id,

                    CreatedByAccountUserId =
                        accountUser.Id,

                    Status =
                        RfqStatus.Draft,

                    DesiredDeliveryDate =
                        request.DesiredDeliveryDate,

                    DeadlineAt =
                        request.DeadlineAt,

                    Notes =
                        Normalize(
                            request.Notes
                        ),

                    CreatedAt =
                        now,

                    Items =
                        request.Items
                            .Select(
                                item =>
                                    new RfqItem
                                    {
                                        ProductId =
                                            item.ProductId,

                                        ProductVariantId =
                                            item.ProductVariantId,

                                        UnitId =
                                            item.UnitId,

                                        Quantity =
                                            item.Quantity,

                                        Notes =
                                            Normalize(
                                                item.Notes
                                            )
                                    }
                            )
                            .ToList(),

                    Categories =
                        request.CategoryIds
                            .Distinct()
                            .Select(
                                categoryId =>
                                    new RfqCategory
                                    {
                                        CategoryId =
                                            categoryId
                                    }
                            )
                            .ToList()
                };


            await _unitOfWork.Rfqs.InsertAsync(rfq);


            await _unitOfWork.SaveChangeAsync();


            return Result.Success(
                rfq.Id
            );
        }
        public async Task<Result<RfqDetailsResponse>> GetByIdAsync(int rfqId, int currentUserId)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project,
                        x => x.Items,
                        x => x.Attachments,
                        x => x.Categories
                    );


            if (rfq is null)
            {
                return Result
                    .Failure<RfqDetailsResponse>(
                        RfqErrors.NotFound
                    );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    rfq.Project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result
                    .Failure<RfqDetailsResponse>(
                        RfqErrors.AccessDenied
                    );
            }


            return Result.Success(MapDetails(rfq));
        }
        public async Task<Result<List<RfqSummaryResponse>>> GetProjectRfqsAsync(int projectId, int currentUserId)
        {
            var project =
                await _unitOfWork
                    .Projects
                    .GetByIdAsync(
                        projectId
                    );


            if (project is null)
            {
                return Result
                    .Failure<List<RfqSummaryResponse>>(
                        RfqErrors.ProjectNotFound
                    );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result
                    .Failure<List<RfqSummaryResponse>>(
                        RfqErrors.AccessDenied
                    );
            }


            var rfqs =
                await _unitOfWork
                    .Rfqs
                    .GetAllAsync(
                        filter:
                            x =>
                                x.ProjectId ==
                                projectId,

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.CreatedAt
                                ),

                        includes:
                        [
                            x => x.Items,
                    x => x.Attachments
                        ]
                    );


            return Result.Success(
                rfqs
                    .Select(
                        x =>
                            new RfqSummaryResponse(
                                x.Id,
                                x.Number,
                                x.ProjectId,
                                x.Status,
                                x.DesiredDeliveryDate,
                                x.DeadlineAt,
                                x.Items.Count,
                                x.Attachments.Count,
                                x.CreatedAt
                            )
                    )
                    .ToList()
            );
        }
        public async Task<Result> UpdateDraftAsync(int rfqId, int currentUserId, UpdateRfqRequest request)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            if (rfq.Status !=
                RfqStatus.Draft)
            {
                return Result.Failure(
                    RfqErrors.OnlyDraftCanBeEdited
                );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    rfq.Project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result.Failure(
                    RfqErrors.AccessDenied
                );
            }


            var validation =
                await ValidateDraftDataAsync(
                    request.DeadlineAt,
                    request.DesiredDeliveryDate,
                    request.CategoryIds,
                    request.Items
                );


            if (validation.IsFailure)
            {
                return validation;
            }


            var trackedRfq =
                await _unitOfWork
                    .Rfqs
                    .FindAsync(
                        rfq.Id
                    );


            if (trackedRfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            trackedRfq.DesiredDeliveryDate =
                request.DesiredDeliveryDate;

            trackedRfq.DeadlineAt =
                request.DeadlineAt;

            trackedRfq.Notes =
                Normalize(
                    request.Notes
                );

            trackedRfq.UpdatedAt =
                DateTime.UtcNow;


            // ==========================================
            // Replace categories
            // ==========================================

            await _unitOfWork
                .RfqCategories
                .ReplaceAsync(
                    rfq.Id,
                    request.CategoryIds
                );


            // ==========================================
            // Replace items
            // ==========================================

            var oldItems =
                await _unitOfWork
                    .RfqItems
                    .GetAllAsync(
                        filter:
                            x => x.RfqId == rfq.Id
                    );


            foreach (var oldItem in oldItems)
            {
                var trackedItem =
                    await _unitOfWork
                        .RfqItems
                        .FindAsync(
                            oldItem.Id
                        );


                if (trackedItem is not null)
                {
                    _unitOfWork
                        .RfqItems
                        .Delete(
                            trackedItem
                        );
                }
            }


            var newItems =
                request.Items
                    .Select(
                        item =>
                            new RfqItem
                            {
                                RfqId =
                                    rfq.Id,

                                ProductId =
                                    item.ProductId,

                                ProductVariantId =
                                    item.ProductVariantId,

                                UnitId =
                                    item.UnitId,

                                Quantity =
                                    item.Quantity,

                                Notes =
                                    Normalize(
                                        item.Notes
                                    )
                            }
                    )
                    .ToList();


            if (newItems.Count > 0)
            {
                await _unitOfWork
                    .RfqItems
                    .InsertListAsync(
                        newItems
                    );
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        public async Task<Result<RfqAttachmentResponse>> AddAttachmentAsync(int rfqId, int currentUserId, UploadRfqAttachmentRequest request)
        {
            const long maxFileSize =
                10 * 1024 * 1024;


            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project
                    );


            if (rfq is null)
            {
                return Result
                    .Failure<RfqAttachmentResponse>(
                        RfqErrors.NotFound
                    );
            }


            if (rfq.Status !=
                RfqStatus.Draft)
            {
                return Result
                    .Failure<RfqAttachmentResponse>(
                        RfqErrors.OnlyDraftCanBeEdited
                    );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    rfq.Project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result
                    .Failure<RfqAttachmentResponse>(
                        RfqErrors.AccessDenied
                    );
            }


            if (request.Length <= 0
                ||
                request.Length > maxFileSize)
            {
                return Result
                    .Failure<RfqAttachmentResponse>(
                        RfqErrors.AttachmentTooLarge
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
            ".xlsx",
            ".xls",
            ".docx",
            ".jpg",
            ".jpeg",
            ".png"
                };


            if (!allowedExtensions.Contains(
                    extension))
            {
                return Result
                    .Failure<RfqAttachmentResponse>(
                        RfqErrors.UnsupportedAttachment
                    );
            }


            var filePath =
                await _fileStorageService
                    .SaveAsync(
                        request.Content,
                        request.FileName,
                        $"uploads/rfqs/{rfq.Id}"
                    );


            var attachment =
                new RfqAttachment
                {
                    RfqId =
                        rfq.Id,

                    Type =
                        request.Type,

                    FilePath =
                        filePath,

                    OriginalFileName =
                        request.FileName,

                    ContentType =
                        request.ContentType,

                    FileSizeBytes =
                        request.Length,

                    UploadedAt =
                        DateTime.UtcNow
                };


            await _unitOfWork
                .RfqAttachments
                .InsertAsync(
                    attachment
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                MapAttachment(
                    attachment
                )
            );
        }

        public async Task<Result> RemoveAttachmentAsync(int rfqId, int attachmentId, int currentUserId)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            if (rfq.Status !=
                RfqStatus.Draft)
            {
                return Result.Failure(
                    RfqErrors.OnlyDraftCanBeEdited
                );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    rfq.Project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result.Failure(
                    RfqErrors.AccessDenied
                );
            }


            var attachment =
                await _unitOfWork
                    .RfqAttachments
                    .GetByIdAsync(
                        attachmentId
                    );


            if (attachment is null
                ||
                attachment.RfqId != rfq.Id)
            {
                return Result.Failure(
                    RfqErrors.AttachmentNotFound
                );
            }


            var trackedAttachment =
                await _unitOfWork
                    .RfqAttachments
                    .FindAsync(
                        attachment.Id
                    );


            if (trackedAttachment is null)
            {
                return Result.Failure(
                    RfqErrors.AttachmentNotFound
                );
            }


            _unitOfWork
                .RfqAttachments
                .Delete(
                    trackedAttachment
                );


            await _unitOfWork
                .SaveChangeAsync();


            await _fileStorageService
                .DeleteAsync(
                    attachment.FilePath
                );


            return Result.Success();
        }

        public async Task<Result> CloseAsync(int rfqId, int currentUserId)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            if (rfq.Status !=
                RfqStatus.Open)
            {
                return Result.Failure(
                    RfqErrors.InvalidStatus
                );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    rfq.Project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result.Failure(
                    RfqErrors.AccessDenied
                );
            }


            var trackedRfq =
                await _unitOfWork
                    .Rfqs
                    .FindAsync(
                        rfq.Id
                    );


            if (trackedRfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            var now =
                DateTime.UtcNow;


            trackedRfq.Status =
                RfqStatus.Closed;

            trackedRfq.ClosedAt =
                now;

            trackedRfq.UpdatedAt =
                now;


            await ExpireOpenInvitationsAsync(
                rfq.Id
            );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }

        public async Task<Result> CancelAsync(int rfqId, int currentUserId)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            if (rfq.Status ==
                    RfqStatus.Awarded
                ||
                rfq.Status ==
                    RfqStatus.Cancelled)
            {
                return Result.Failure(
                    RfqErrors.InvalidStatus
                );
            }


            var accountUser =
                await GetProjectAccountUserAsync(
                    rfq.Project,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result.Failure(
                    RfqErrors.AccessDenied
                );
            }


            var trackedRfq =
                await _unitOfWork
                    .Rfqs
                    .FindAsync(
                        rfq.Id
                    );


            if (trackedRfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            trackedRfq.Status =
                RfqStatus.Cancelled;

            trackedRfq.UpdatedAt =
                DateTime.UtcNow;


            await ExpireOpenInvitationsAsync(
                rfq.Id
            );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }
        public async Task<Result> SubmitAsync(int rfqId, int currentUserId)
        {
            // ========================================
            // Get RFQ For Validation
            // ========================================

            var rfq =
                await _unitOfWork.Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project,
                        x => x.Categories,
                        x => x.Items,
                        x => x.Attachments,
                        x => x.Invitations
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            // ========================================
            // RFQ must be Draft
            // ========================================

            if (rfq.Status != RfqStatus.Draft)
            {
                return Result.Failure(
                    RfqErrors.NotDraft
                );
            }


            // ========================================
            // Get Customer Account User
            // ========================================

            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                                currentUserId
                            &&
                            x.CustomerAccountId ==
                                rfq.Project
                                    .CustomerAccountId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return Result.Failure(
                    RfqErrors
                        .UnauthorizedProjectAccess
                );
            }


            // ========================================
            // Check Project Access
            // ========================================

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
                                    rfq.ProjectId
                                &&
                                x.CustomerAccountUserId ==
                                    accountUser.Id
                                &&
                                x.IsActive
                        );
            }


            if (!hasProjectAccess)
            {
                return Result.Failure(
                    RfqErrors
                        .UnauthorizedProjectAccess
                );
            }


            // ========================================
            // Categories Validation
            // ========================================

            if (rfq.Categories.Count == 0)
            {
                return Result.Failure(
                    RfqErrors.CategoriesRequired
                );
            }


            // ========================================
            // Content Validation
            // ========================================

            if (rfq.Items.Count == 0 &&
                rfq.Attachments.Count == 0)
            {
                return Result.Failure(
                    RfqErrors.ContentRequired
                );
            }


            // ========================================
            // Deadline Validation
            // ========================================

            if (!rfq.DeadlineAt.HasValue ||
                rfq.DeadlineAt.Value <=
                    DateTime.UtcNow)
            {
                return Result.Failure(
                    RfqErrors.InvalidDeadline
                );
            }


            // ========================================
            // Find Matching Vendors
            // ========================================

            var matchingResult =
                await _rfqMatchingService
                    .FindMatchingVendorIdsAsync(
                        rfq.Id
                    );


            if (matchingResult.IsFailure)
            {
                return Result.Failure(
                    matchingResult.Error
                );
            }


            var vendorIds =
                matchingResult.Value!;


            // ========================================
            // Existing Invitations
            // ========================================

            var existingVendorIds =
                rfq.Invitations
                    .Select(x => x.VendorId)
                    .ToHashSet();


            var newInvitations =
                vendorIds
                    .Where(
                        vendorId =>
                            !existingVendorIds
                                .Contains(vendorId)
                    )
                    .Select(
                        vendorId =>
                            new RfqInvitation
                            {
                                RfqId = rfq.Id,

                                VendorId = vendorId,

                                Status =
                                    RfqInvitationStatus
                                        .Sent,

                                SentAt =
                                    DateTime.UtcNow
                            }
                    )
                    .ToList();


            if (newInvitations.Count > 0)
            {
                await _unitOfWork
                    .RfqInvitations
                    .InsertListAsync(
                        newInvitations
                    );
            }


            // ========================================
            // Get Tracked RFQ
            // ========================================

            var trackedRfq =
                await _unitOfWork.Rfqs
                    .FindAsync(rfq.Id);


            if (trackedRfq is null)
            {
                return Result.Failure(
                    RfqErrors.NotFound
                );
            }


            // ========================================
            // Open RFQ
            // ========================================

            var now = DateTime.UtcNow;

            trackedRfq.Status =
                RfqStatus.Open;

            trackedRfq.SubmittedAt = now;

            trackedRfq.UpdatedAt = now;


            // FindAsync already returns tracked entity,
            // so Update() is not required here.


            // ========================================
            // Save All Changes
            // ========================================

            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }




        private async Task<CustomerAccountUser?> GetProjectAccountUserAsync(Project project, int currentUserId)
        {
            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.CustomerAccountId ==
                                project.CustomerAccountId
                            &&
                            x.UserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return null;
            }


            if (accountUser.Role ==
                CustomerAccountUserRole.Owner)
            {
                return accountUser;
            }


            var hasAssignment =
                await _unitOfWork
                    .ProjectMembers
                    .AnyAsync(
                        x =>
                            x.ProjectId ==
                                project.Id
                            &&
                            x.CustomerAccountUserId ==
                                accountUser.Id
                            &&
                            x.IsActive
                    );


            return hasAssignment
                ? accountUser
                : null;
        }

        private async Task<Result> ValidateDraftDataAsync(DateTime deadlineAt, DateOnly? desiredDeliveryDate, List<int> categoryIds, List<CreateRfqItemRequest> items)
        {
            if (deadlineAt <=
                DateTime.UtcNow)
            {
                return Result.Failure(
                    RfqErrors.InvalidDeadline
                );
            }


            if (desiredDeliveryDate.HasValue
                &&
                desiredDeliveryDate.Value <
                    DateOnly.FromDateTime(
                        DateTime.UtcNow
                    ))
            {
                return Result.Failure(
                    RfqErrors.InvalidDeliveryDate
                );
            }


            var distinctCategoryIds =
                categoryIds
                    .Distinct()
                    .ToList();


            if (distinctCategoryIds.Count > 0)
            {
                var categories =
                    await _unitOfWork
                        .Categories
                        .GetAllAsync(
                            filter:
                                x =>
                                    distinctCategoryIds
                                        .Contains(x.Id)
                                    &&
                                    x.IsActive
                        );


                if (categories.Count !=
                    distinctCategoryIds.Count)
                {
                    return Result.Failure(
                        RfqErrors.InvalidCategory
                    );
                }
            }


            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                {
                    return Result.Failure(
                        RfqErrors.InvalidQuantity
                    );
                }


                var product =
                    await _unitOfWork
                        .Products
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id ==
                                    item.ProductId
                                &&
                                x.IsActive
                        );


                if (product is null)
                {
                    return Result.Failure(
                        RfqErrors.ProductNotFound
                    );
                }


                var unit =
                    await _unitOfWork
                        .Units
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id ==
                                    item.UnitId
                                &&
                                x.IsActive
                        );


                if (unit is null)
                {
                    return Result.Failure(
                        RfqErrors.UnitNotFound
                    );
                }


                if (item.ProductVariantId.HasValue)
                {
                    var variant =
                        await _unitOfWork
                            .ProductVariants
                            .FirstOrDefaultAsync(
                                x =>
                                    x.Id ==
                                        item.ProductVariantId.Value
                                    &&
                                    x.ProductId ==
                                        item.ProductId
                                    &&
                                    x.IsActive
                            );


                    if (variant is null)
                    {
                        return Result.Failure(
                            RfqErrors.VariantNotFound
                        );
                    }
                }
            }


            return Result.Success();
        }
        private async Task ExpireOpenInvitationsAsync(int rfqId)
        {
            var invitations =
                await _unitOfWork
                    .RfqInvitations
                    .GetAllAsync(
                        filter:
                            x =>
                                x.RfqId == rfqId
                                &&
                                (
                                    x.Status ==
                                        RfqInvitationStatus.Sent
                                    ||
                                    x.Status ==
                                        RfqInvitationStatus.Viewed
                                )
                    );


            foreach (var invitation in invitations)
            {
                var trackedInvitation =
                    await _unitOfWork
                        .RfqInvitations
                        .FindAsync(
                            invitation.Id
                        );


                if (trackedInvitation is not null)
                {
                    trackedInvitation.Status =
                        RfqInvitationStatus.Expired;
                }
            }
        }

        private static RfqDetailsResponse MapDetails(
    Rfq rfq)
        {
            return new RfqDetailsResponse(
                rfq.Id,
                rfq.Number,
                rfq.ProjectId,
                rfq.Status,
                rfq.DesiredDeliveryDate,
                rfq.DeadlineAt,
                rfq.Notes,
                rfq.CreatedAt,
                rfq.SubmittedAt,
                rfq.ClosedAt,

                rfq.Categories
                    .Select(
                        x => x.CategoryId
                    )
                    .ToList(),

                rfq.Items
                    .Select(
                        x =>
                            new RfqItemResponse(
                                x.Id,
                                x.ProductId,
                                x.ProductVariantId,
                                x.UnitId,
                                x.Quantity,
                                x.Notes
                            )
                    )
                    .ToList(),

                rfq.Attachments
                    .Select(MapAttachment)
                    .ToList()
            );
        }


        private static RfqAttachmentResponse MapAttachment(RfqAttachment attachment)
        {
            return new RfqAttachmentResponse(
                attachment.Id,
                attachment.Type,
                attachment.FilePath,
                attachment.OriginalFileName,
                attachment.ContentType,
                attachment.FileSizeBytes,
                attachment.UploadedAt
            );
        }


        private static string GenerateRfqNumber()
        {
            return
                $"RFQ-{DateTime.UtcNow:yyyyMMdd}-" +
                $"{Guid.NewGuid():N}"[..8]
                    .ToUpperInvariant();
        }


        private static string? Normalize(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }
    }
}