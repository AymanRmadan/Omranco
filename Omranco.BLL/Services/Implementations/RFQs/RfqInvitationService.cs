using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.RfqServices;
using Omranco.Domain.Enums.Rfqs;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.RfqServices
{
    public sealed class RfqInvitationService
        : IRfqInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RfqInvitationService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result> MarkAsViewedAsync(
            int invitationId,
            int currentUserId)
        {
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
                    RfqInvitationErrors.AccessDenied
                );
            }


            var invitation =
                await _unitOfWork.RfqInvitations
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == invitationId &&
                            x.VendorId == vendor.Id,
                        x => x.Rfq
                    );


            if (invitation is null)
            {
                return Result.Failure(
                    RfqInvitationErrors.NotFound
                );
            }


            if (invitation.Rfq.Status !=
                RfqStatus.Open)
            {
                return Result.Failure(
                    RfqInvitationErrors.RfqNotOpen
                );
            }


            // Already viewed / responded
            if (invitation.Status ==
                    RfqInvitationStatus.Viewed ||
                invitation.Status ==
                    RfqInvitationStatus.Responded)
            {
                return Result.Success();
            }


            if (invitation.Status !=
                RfqInvitationStatus.Sent)
            {
                return Result.Failure(
                    RfqInvitationErrors.RfqNotOpen
                );
            }


            var trackedInvitation =
                await _unitOfWork
                    .RfqInvitations
                    .FindAsync(invitation.Id);


            if (trackedInvitation is null)
            {
                return Result.Failure(
                    RfqInvitationErrors.NotFound
                );
            }


            trackedInvitation.Status =
                RfqInvitationStatus.Viewed;

            trackedInvitation.ViewedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }
    }
}