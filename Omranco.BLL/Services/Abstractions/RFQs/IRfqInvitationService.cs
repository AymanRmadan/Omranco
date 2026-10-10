using Omranco.BLL.Commons.ResponseResults;

namespace Omranco.BLL.Services.Abstractions.RfqServices
{
    public interface IRfqInvitationService
    {
        Task<Result> MarkAsViewedAsync(
            int invitationId,
            int currentUserId);
    }
}