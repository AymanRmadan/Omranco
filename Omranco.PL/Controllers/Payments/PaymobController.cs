using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Payments.Paymob.Webhooks;
using Omranco.BLL.Services.Abstractions.PaymentServices;

namespace Omranco.PL.Controllers.Payments;

[ApiController]
[Route("api/payments/paymob")]
public sealed class PaymobController
    : ControllerBase
{
    private readonly IPaymentService _paymentService;


    public PaymobController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }


    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> TransactionProcessed([FromBody] PaymobTransactionCallback callback, [FromQuery] string hmac)
    {
        var result = await _paymentService.HandlePaymobWebhookAsync(callback, hmac);

        //if (result.IsFailure)
        //{
        //    return Problem(result.Error);
        //}

        return result.IsSuccess ? Ok() : result.ToProblem(); ;
    }
}