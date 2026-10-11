namespace Omranco.BLL.DTOS.Payments.Responses;

public sealed record StartPaymentResponse(
    int PaymentAttemptId,
    string CheckoutUrl
);