using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Requests;
using Omranco.BLL.Services.Abstractions.VendorServices;
using Omranco.PL.Models.Vendors;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendor/onboarding")]
[Authorize]
public sealed class VendorOnboardingController
    : ControllerBase
{
    private readonly IVendorService
        _vendorService;


    public VendorOnboardingController(
        IVendorService vendorService)
    {
        _vendorService =
            vendorService;
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateVendorProfileRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .CreateAsync(
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .GetMyProfileAsync(
                    userId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody]
        UpdateVendorProfileRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .UpdateAsync(
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPut("categories")]
    public async Task<IActionResult> SetCategories(
        [FromBody]
        SetVendorCategoriesRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .SetCategoriesAsync(
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPut("service-zones")]
    public async Task<IActionResult> SetZones(
        [FromBody]
        SetVendorZonesRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .SetServiceZonesAsync(
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpGet("document-requirements")]
    public async Task<IActionResult>
        GetDocumentRequirements()
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .GetDocumentRequirementsAsync(
                    userId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPost(
        "documents/{requirementId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(
        int requirementId,
        [FromForm]
        UploadVendorDocumentForm request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        await using var stream =
            request.File.OpenReadStream();


        var serviceRequest =
            new UploadVendorDocumentRequest(
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                stream
            );


        var result =
            await _vendorService
                .UploadDocumentAsync(
                    requirementId,
                    userId,
                    serviceRequest
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPost("submit")]
    public async Task<IActionResult> Submit()
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorService
                .SubmitForReviewAsync(
                    userId
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}