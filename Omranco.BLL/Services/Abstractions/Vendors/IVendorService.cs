using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Requests;
using Omranco.BLL.DTOS.Vendors.Responses;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.Services.Abstractions.VendorServices
{
    public interface IVendorService
    {
        // Vendor

        Task<Result<VendorProfileResponse>> CreateAsync(
            int currentUserId,
            CreateVendorProfileRequest request);

        Task<Result<VendorProfileResponse>> GetMyProfileAsync(
            int currentUserId);

        Task<Result<VendorProfileResponse>> UpdateAsync(
            int currentUserId,
            UpdateVendorProfileRequest request);

        Task<Result> SetCategoriesAsync(
            int currentUserId,
            SetVendorCategoriesRequest request);

        Task<Result> SetServiceZonesAsync(
            int currentUserId,
            SetVendorZonesRequest request);

        Task<Result<List<VendorDocumentRequirementResponse>>>
            GetDocumentRequirementsAsync(
                int currentUserId);

        Task<Result<VendorDocumentResponse>> UploadDocumentAsync(
            int requirementId,
            int currentUserId,
            UploadVendorDocumentRequest request);

        Task<Result> SubmitForReviewAsync(
            int currentUserId);


        // Admin

        Task<Result<List<AdminVendorListItemResponse>>> GetAllAsync(
            VendorStatus? status);

        Task<Result<VendorProfileResponse>> GetByIdAsync(
            int vendorId);

        Task<Result> ApproveAsync(
            int vendorId,
            int currentUserId);

        Task<Result> RejectAsync(
            int vendorId,
            int currentUserId,
            RejectVendorRequest request);

        Task<Result> SuspendAsync(
            int vendorId,
            int currentUserId,
            SuspendVendorRequest request);

        Task<Result> ReactivateAsync(
            int vendorId,
            int currentUserId);

        Task<Result> ApproveDocumentAsync(
            int documentId,
            int currentUserId);

        Task<Result> RejectDocumentAsync(
            int documentId,
            int currentUserId,
            RejectVendorDocumentRequest request);
    }
}