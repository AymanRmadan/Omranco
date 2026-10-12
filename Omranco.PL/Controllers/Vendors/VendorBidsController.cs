using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;
using Omranco.BLL.Services.Abstractions.BidServices;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendor")]
[Authorize]
public sealed class VendorBidsController
    : ControllerBase
{
    private readonly IBidService _bidService;


    public VendorBidsController(
        IBidService bidService)
    {
        _bidService =
            bidService;
    }


    // ==========================================
    // Invitations
    // ==========================================

    [HttpGet("rfq-invitations")]
    public async Task<IActionResult>
        GetInvitations()
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .GetMyInvitationsAsync(
                    userId
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpGet("rfq-invitations/{rfqId:int}")]
    public async Task<IActionResult>
        GetInvitation(
            int rfqId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .GetInvitationAsync(
                    rfqId,
                    userId
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost(
        "rfq-invitations/{rfqId:int}/decline")]
    public async Task<IActionResult>
        DeclineInvitation(
            int rfqId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .DeclineInvitationAsync(
                    rfqId,
                    userId
                );

        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    // ==========================================
    // Bid
    // ==========================================

    [HttpPost("rfqs/{rfqId:int}/bid")]
    public async Task<IActionResult> SubmitBid(
        int rfqId,
        [FromBody]
        SubmitBidRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .SubmitAsync(
                    rfqId,
                    userId,
                    request
                );



        return result.IsSuccess ? Ok(new
        {
            BidId =
                    result.Value
        }) : result.ToProblem();
    }


    [HttpGet("rfqs/{rfqId:int}/bid")]
    public async Task<IActionResult>
        GetMyBid(
            int rfqId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .GetMyBidAsync(
                    rfqId,
                    userId
                );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPut("rfqs/{rfqId:int}/bid")]
    public async Task<IActionResult>
        UpdateBid(
            int rfqId,
            [FromBody]
            UpdateBidRequest request)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .UpdateAsync(
                    rfqId,
                    userId,
                    request
                );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost(
        "rfqs/{rfqId:int}/bid/withdraw")]
    public async Task<IActionResult>
        WithdrawBid(
            int rfqId)
    {
        var userId =
            User.GetUserId();


        if (userId == 0)
            return Unauthorized();


        var result =
            await _bidService
                .WithdrawAsync(
                    rfqId,
                    userId
                );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}