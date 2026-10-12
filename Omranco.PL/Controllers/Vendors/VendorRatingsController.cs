using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.RatingServices;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendor/ratings")]
[Authorize]
public sealed class VendorRatingsController
    : ControllerBase
{
    private readonly IVendorRatingService
        _vendorRatingService;


    public VendorRatingsController(
        IVendorRatingService vendorRatingService)
    {
        _vendorRatingService =
            vendorRatingService;
    }


    [HttpGet]
    public async Task<IActionResult> GetMyRatings()
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorRatingService
                .GetMyVendorRatingsAsync(
                    userId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}