using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Rfqs;

namespace Omranco.BLL.Services.Abstractions.RfqServices;

public interface IRfqService
{
    Task<Result<int>> CreateDraftAsync(
        int projectId,
        int currentUserId,
        CreateRfqRequest request);

    Task<Result<RfqDetailsResponse>> GetByIdAsync(
        int rfqId,
        int currentUserId);

    Task<Result<List<RfqSummaryResponse>>> GetProjectRfqsAsync(
        int projectId,
        int currentUserId);

    Task<Result> UpdateDraftAsync(
        int rfqId,
        int currentUserId,
        UpdateRfqRequest request);

    Task<Result<RfqAttachmentResponse>> AddAttachmentAsync(
        int rfqId,
        int currentUserId,
        UploadRfqAttachmentRequest request);

    Task<Result> RemoveAttachmentAsync(
        int rfqId,
        int attachmentId,
        int currentUserId);

    Task<Result> SubmitAsync(
        int rfqId,
        int currentUserId);

    Task<Result> CloseAsync(
        int rfqId,
        int currentUserId);

    Task<Result> CancelAsync(
        int rfqId,
        int currentUserId);
}