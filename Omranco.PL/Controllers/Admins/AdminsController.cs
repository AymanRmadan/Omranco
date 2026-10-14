using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Requests;
using Omranco.BLL.Services.Abstractions.VendorServices;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.PL.Controllers.Admin;

[ApiController]
[Route("api/admin/vendors")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class AdminVendorsController : ControllerBase
{
    private readonly IVendorService
        _vendorService;


    public AdminVendorsController(
        IVendorService vendorService)
    {
        _vendorService =
            vendorService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        VendorStatus? status)
    {
        var result =
            await _vendorService
                .GetAllAsync(
                    status
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet("{vendorId:int}")]
    public async Task<IActionResult> GetById(
        int vendorId)
    {
        var result =
            await _vendorService
                .GetByIdAsync(
                    vendorId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPost("{vendorId:int}/approve")]
    public async Task<IActionResult> Approve(
        int vendorId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .ApproveAsync(
                    vendorId,
                    userId
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPost("{vendorId:int}/reject")]
    public async Task<IActionResult> Reject(
        int vendorId,
        [FromBody]
        RejectVendorRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .RejectAsync(
                    vendorId,
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPost("{vendorId:int}/suspend")]
    public async Task<IActionResult> Suspend(
        int vendorId,
        [FromBody]
        SuspendVendorRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .SuspendAsync(
                    vendorId,
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPost("{vendorId:int}/reactivate")]
    public async Task<IActionResult> Reactivate(
        int vendorId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .ReactivateAsync(
                    vendorId,
                    userId
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPost(
        "documents/{documentId:int}/approve")]
    public async Task<IActionResult>
        ApproveDocument(
            int documentId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .ApproveDocumentAsync(
                    documentId,
                    userId
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPost(
        "documents/{documentId:int}/reject")]
    public async Task<IActionResult>
        RejectDocument(
            int documentId,
            [FromBody]
            RejectVendorDocumentRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .RejectDocumentAsync(
                    documentId,
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}