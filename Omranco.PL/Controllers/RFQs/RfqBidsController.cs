using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.BidServices;

namespace Omranco.PL.Controllers.Rfqs;

[ApiController]
[Route("api/rfqs/{rfqId:int}/bids")]
[Authorize]
public sealed class RfqBidsController
    : ControllerBase
{
    private readonly IBidService _bidService;


    public RfqBidsController(
        IBidService bidService)
    {
        _bidService =
            bidService;
    }


    [HttpGet("comparison")]
    public async Task<IActionResult>
        GetComparison(
            int rfqId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .GetComparisonAsync(
                    rfqId,
                    userId
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost("{bidId:int}/accept")]
    public async Task<IActionResult>
        Accept(
            int rfqId,
            int bidId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .AcceptAsync(
                    rfqId,
                    bidId,
                    userId
                );



        return result.IsSuccess ? Ok(new
        {
            OrderId =
                    result.Value
        }) : result.ToProblem();
    }
}