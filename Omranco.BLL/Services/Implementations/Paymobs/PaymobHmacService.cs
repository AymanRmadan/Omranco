using Omranco.BLL.DTOS.Payments.Paymob.Webhooks;
using Omranco.BLL.Services.Abstractions.PaymentServices;
using System.Globalization;

namespace Omranco.BLL.Services.Implementations.PaymentServices;

public sealed class PaymobHmacService
    : IPaymobHmacService
{
    private readonly PaymobOptions _options;

    public PaymobHmacService(
        IOptions<PaymobOptions> options)
    {
        _options = options.Value;
    }


    public bool IsValidTransactionHmac(
        PaymobTransactionObject transaction,
        string receivedHmac)
    {
        if (string.IsNullOrWhiteSpace(receivedHmac))
            return false;


        var concatenated =
            string.Concat(
                ToHmacValue(transaction.AmountCents),
                ToHmacValue(transaction.CreatedAt),
                ToHmacValue(transaction.Currency),
                ToHmacValue(transaction.ErrorOccurred),
                ToHmacValue(transaction.HasParentTransaction),
                ToHmacValue(transaction.Id),
                ToHmacValue(transaction.IntegrationId),
                ToHmacValue(transaction.Is3DSecure),
                ToHmacValue(transaction.IsAuth),
                ToHmacValue(transaction.IsCapture),
                ToHmacValue(transaction.IsRefunded),
                ToHmacValue(transaction.IsStandalonePayment),
                ToHmacValue(transaction.IsVoided),
                ToHmacValue(transaction.Order.Id),
                ToHmacValue(transaction.Owner),
                ToHmacValue(transaction.Pending),
                ToHmacValue(transaction.SourceData?.Pan),
                ToHmacValue(transaction.SourceData?.SubType),
                ToHmacValue(transaction.SourceData?.Type),
                ToHmacValue(transaction.Success)
            );


        using var hmac =
            new HMACSHA512(
                Encoding.UTF8.GetBytes(
                    _options.HmacSecret
                )
            );


        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(
                    concatenated
                )
            );


        byte[] receivedBytes;

        try
        {
            receivedBytes =
                Convert.FromHexString(
                    receivedHmac
                );
        }
        catch (FormatException)
        {
            return false;
        }


        if (hash.Length != receivedBytes.Length)
            return false;


        return CryptographicOperations.FixedTimeEquals(
            hash,
            receivedBytes
        );
    }


    private static string ToHmacValue(
        object? value)
    {
        return value switch
        {
            null => string.Empty,

            bool boolean =>
                boolean
                    ? "true"
                    : "false",

            IFormattable formattable =>
                formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture
                ),

            _ =>
                value.ToString()
                ?? string.Empty
        };
    }
}