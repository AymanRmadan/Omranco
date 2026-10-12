using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Ratings.Requests;
using Omranco.BLL.Services.Abstractions.RatingServices;

namespace Omranco.PL.Controllers.Customers;

[ApiController]
[Route("api/orders/{orderId:int}/rating")]
[Authorize]
public sealed class OrderRatingsController
    : ControllerBase
{
    private readonly IVendorRatingService
        _vendorRatingService;


    public OrderRatingsController(
        IVendorRatingService vendorRatingService)
    {
        _vendorRatingService =
            vendorRatingService;
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        int orderId,
        [FromBody]
        CreateVendorRatingRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorRatingService
                .CreateAsync(
                    orderId,
                    userId,
                    request
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet]
    public async Task<IActionResult> Get(
        int orderId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _vendorRatingService
                .GetOrderRatingAsync(
                    orderId,
                    userId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}