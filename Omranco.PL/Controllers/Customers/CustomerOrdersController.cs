using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.Domain.Enums.Orders;

namespace Omranco.PL.Controllers.Customers;

[ApiController]
[Route("api/customer-accounts")]
[Authorize]
public sealed class CustomerOrdersController
    : ControllerBase
{
    private readonly IOrderService _orderService;

    public CustomerOrdersController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }


    [HttpGet("{accountId:int}/orders")]
    public async Task<IActionResult> GetOrders(
        int accountId,
        [FromQuery] OrderStatus? status)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _orderService
                .GetCustomerOrdersAsync(
                    accountId,
                    userId,
                    status
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet("orders/{orderId:int}")]
    public async Task<IActionResult> GetDetails(
        int orderId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _orderService
                .GetCustomerOrderDetailsAsync(
                    orderId,
                    userId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}