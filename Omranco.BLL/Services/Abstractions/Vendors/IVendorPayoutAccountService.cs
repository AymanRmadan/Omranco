using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.PayoutAccounts;

namespace Omranco.BLL.Services.Abstractions.VendorServices;

public interface IVendorPayoutAccountService
{
    Task<Result<List<VendorPayoutAccountResponse>>>
        GetMyAccountsAsync(
            int currentUserId);

    Task<Result<VendorPayoutAccountResponse>>
        CreateAsync(
            int currentUserId,
            CreateVendorPayoutAccountRequest request);

    Task<Result<VendorPayoutAccountResponse>>
        UpdateAsync(
            int accountId,
            int currentUserId,
            UpdateVendorPayoutAccountRequest request);

    Task<Result>
        SetDefaultAsync(
            int accountId,
            int currentUserId);

    Task<Result>
        VerifyAsync(
            int accountId);
}