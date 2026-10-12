using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.RatingServices;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendors")]
[Authorize]
public sealed class VendorPublicRatingsController
    : ControllerBase
{
    private readonly IVendorRatingService
        _vendorRatingService;


    public VendorPublicRatingsController(
        IVendorRatingService vendorRatingService)
    {
        _vendorRatingService =
            vendorRatingService;
    }


    [HttpGet("{vendorId:int}/ratings")]
    public async Task<IActionResult> GetRatings(
        int vendorId)
    {
        var result =
            await _vendorRatingService
                .GetVendorRatingsAsync(
                    vendorId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}