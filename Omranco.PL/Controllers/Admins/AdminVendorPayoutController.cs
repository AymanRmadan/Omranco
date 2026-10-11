using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Payouts;
using Omranco.BLL.Services.Abstractions.VendorServices;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.PL.Controllers.Admin;

[ApiController]
[Route("api/admin/vendor-payouts")]
[Authorize]
public sealed class AdminVendorPayoutController
    : ControllerBase
{
    private readonly IVendorPayoutService
        _vendorPayoutService;

    public AdminVendorPayoutController(
        IVendorPayoutService vendorPayoutService)
    {
        _vendorPayoutService =
            vendorPayoutService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        VendorPayoutRequestStatus? status)
    {
        var result =
            await _vendorPayoutService
                .GetAllAsync(status);


        return result.IsSuccess ? Ok() : result.ToProblem();

    }


    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody]
        ApproveVendorPayoutRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorPayoutService
                .ApproveAsync(
                    id,
                    userId,
                    request
                );


        return result.IsSuccess ? Ok() : result.ToProblem();

    }


    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody]
        RejectVendorPayoutRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorPayoutService
                .RejectAsync(
                    id,
                    userId,
                    request
                );

        return result.IsSuccess ? Ok() : result.ToProblem();

    }


    [HttpPost("{id:int}/processing")]
    public async Task<IActionResult> StartProcessing(
        int id,
        [FromBody]
        ProcessVendorPayoutRequest request)
    {
        var result =
            await _vendorPayoutService
                .StartProcessingAsync(
                    id,
                    request
                );


        return result.IsSuccess ? Ok() : result.ToProblem();

    }


    [HttpPost("{id:int}/paid")]
    public async Task<IActionResult> MarkPaid(
        int id)
    {
        var result =
            await _vendorPayoutService
                .MarkPaidAsync(id);


        return result.IsSuccess ? Ok() : result.ToProblem();

    }


    [HttpPost("{id:int}/failed")]
    public async Task<IActionResult> MarkFailed(
        int id,
        [FromBody]
        FailVendorPayoutRequest request)
    {
        var result =
            await _vendorPayoutService
                .MarkFailedAsync(
                    id,
                    request
                );


        return result.IsSuccess ? Ok() : result.ToProblem();

    }
}