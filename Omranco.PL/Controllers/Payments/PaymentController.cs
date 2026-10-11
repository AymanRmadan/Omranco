using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Payments.Requests;
using Omranco.BLL.Services.Abstractions.PaymentServices;

namespace Omranco.PL.Controllers.Payments;

[ApiController]
[Route("api/orders/{orderId:int}/payment")]
[Authorize]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }


    [HttpPost("start")]
    public async Task<IActionResult> StartPayment(int orderId, [FromBody] StartPaymentRequest request)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == 0)
            return Unauthorized();

        var result = await _paymentService.StartPaymentAsync(orderId, currentUserId, request);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }
}