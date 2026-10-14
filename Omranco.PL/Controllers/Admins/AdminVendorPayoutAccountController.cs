using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.VendorServices;

namespace Omranco.PL.Controllers.Admin;

[ApiController]
[Route("api/admin/vendor-payout-accounts")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class AdminVendorPayoutAccountController
    : ControllerBase
{
    private readonly IVendorPayoutAccountService
        _service;


    public AdminVendorPayoutAccountController(
        IVendorPayoutAccountService service)
    {
        _service = service;
    }


    [HttpPost("{accountId:int}/verify")]
    public async Task<IActionResult> Verify(
        int accountId)
    {
        var result =
            await _service
                .VerifyAsync(
                    accountId
                );

        return result.IsSuccess ? Ok() : result.ToProblem();

    }
}