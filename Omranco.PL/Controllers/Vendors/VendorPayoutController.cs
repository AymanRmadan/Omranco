using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Payouts;
using Omranco.BLL.Services.Abstractions.VendorServices;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendor/payouts")]
[Authorize]
public sealed class VendorPayoutController
    : ControllerBase
{
    private readonly IVendorPayoutService _vendorPayoutService;

    public VendorPayoutController(
        IVendorPayoutService vendorPayoutService)
    {
        _vendorPayoutService = vendorPayoutService;
    }


    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallet()
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result = await _vendorPayoutService.GetWalletSummaryAsync(userId);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();


    }


    [HttpGet]
    public async Task<IActionResult>
        GetMyPayouts()
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorPayoutService
                .GetMyPayoutsAsync(
                    userId
                );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }


    [HttpPost]
    public async Task<IActionResult>
        RequestPayout(
            [FromBody]
            CreateVendorPayoutRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorPayoutService
                .RequestAsync(
                    userId,
                    request
                );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }


    [HttpPost("{payoutRequestId:int}/cancel")]
    public async Task<IActionResult>
        Cancel(
            int payoutRequestId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorPayoutService
                .CancelAsync(
                    payoutRequestId,
                    userId
                );


        return result.IsSuccess ? Ok() : result.ToProblem();

    }
}