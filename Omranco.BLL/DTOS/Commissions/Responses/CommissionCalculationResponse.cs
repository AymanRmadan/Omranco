namespace Omranco.BLL.DTOS.Commissions.Responses
{
    public sealed record CommissionCalculationResponse(
        decimal Rate,
        decimal CommissionAmount,
        decimal VendorNetAmount
    );
}