using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.PayoutAccounts;
using Omranco.BLL.Services.Abstractions.VendorServices;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendor/payout-accounts")]
[Authorize]
public sealed class VendorPayoutAccountController
    : ControllerBase
{
    private readonly IVendorPayoutAccountService
        _service;


    public VendorPayoutAccountController(
        IVendorPayoutAccountService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .GetMyAccountsAsync(
                    userId
                );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }


    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateVendorPayoutAccountRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .CreateAsync(
                    userId,
                    request
                );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }


    [HttpPut("{accountId:int}")]
    public async Task<IActionResult> Update(
        int accountId,
        [FromBody]
        UpdateVendorPayoutAccountRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .UpdateAsync(
                    accountId,
                    userId,
                    request
                );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }


    [HttpPost("{accountId:int}/default")]
    public async Task<IActionResult> SetDefault(
        int accountId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .SetDefaultAsync(
                    accountId,
                    userId
                );


        return result.IsSuccess ? Ok() : result.ToProblem();

    }
}