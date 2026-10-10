using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.RfqServices;
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

        private readonly IRfqMatchingService
            _rfqMatchingService;


        public RfqService(
            IUnitOfWork unitOfWork,
            IRfqMatchingService rfqMatchingService)
        {
            _unitOfWork = unitOfWork;

            _rfqMatchingService =
                rfqMatchingService;
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
    }
}