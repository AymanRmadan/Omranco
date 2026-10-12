using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.Domain.Enums.Orders;

namespace Omranco.PL.Controllers.Vendors;

[ApiController]
[Route("api/vendor/orders")]
[Authorize]
public sealed class VendorOrdersController
    : ControllerBase
{
    private readonly IOrderService _orderService;

    public VendorOrdersController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }


    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderStatus? status)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _orderService
                .GetVendorOrdersAsync(
                    userId,
                    status
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetDetails(
        int orderId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _orderService
                .GetVendorOrderDetailsAsync(
                    orderId,
                    userId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPost("{orderId:int}/status/{status}")]
    public async Task<IActionResult> UpdateStatus(
        int orderId,
        OrderStatus status)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _orderService
                .UpdateStatusAsync(
                    orderId,
                    userId,
                    status
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}