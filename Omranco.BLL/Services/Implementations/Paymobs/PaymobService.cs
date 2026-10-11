using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Payments.Paymob;
using Omranco.BLL.DTOS.Payments.Paymob.Internal;
using Omranco.BLL.Services.Abstractions.PaymentServices;
using Omranco.Domain.Enums.Payments;
using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace Omranco.BLL.Services.Implementations.PaymentServices;

public sealed class PaymobService : IPaymobService
{
    private readonly HttpClient _httpClient;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobService> _logger;

    public PaymobService(
        HttpClient httpClient,
        IOptions<PaymobOptions> options,
        ILogger<PaymobService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }


    public async Task<Result<CreatePaymobIntentionResponse>> CreateIntentionAsync(CreatePaymobIntentionRequest request)
    {
        // ==========================================
        // Validate
        // ==========================================

        if (request.Amount <= 0)
        {
            return Result
                .Failure<CreatePaymobIntentionResponse>(
                    PaymobErrors.InvalidAmount
                );
        }


        var integrationId =
            GetIntegrationId(
                request.PaymentMethod
            );


        if (!integrationId.HasValue)
        {
            return Result
                .Failure<CreatePaymobIntentionResponse>(
                    PaymobErrors.UnsupportedPaymentMethod
                );
        }


        // ==========================================
        // Convert EGP -> Piastres
        // ==========================================

        var amountInCents =
            ConvertToCents(
                request.Amount
            );


        // ==========================================
        // Create Paymob Request
        // ==========================================

        var apiRequest =
            new PaymobCreateIntentionApiRequest(
                Amount:
                    amountInCents,

                Currency:
                    request.Currency,

                PaymentMethods:
                [
                    integrationId.Value
                ],

                Items:
                [
                    new PaymobItemApiRequest(
                        Name:
                            request.ItemName,

                        Amount:
                            amountInCents,

                        Description:
                            request.ItemDescription
                            ?? string.Empty,

                        Quantity:
                            1
                    )
                ],

                BillingData:
                    new PaymobBillingDataApiRequest(
                        FirstName:
                            request.BillingData.FirstName,

                        LastName:
                            request.BillingData.LastName,

                        PhoneNumber:
                            request.BillingData.PhoneNumber,

                        Email:
                            request.BillingData.Email,

                        City:
                            request.BillingData.City,

                        Country:
                            request.BillingData.Country,

                        State:
                            request.BillingData.State,

                        Street:
                            request.BillingData.Street,

                        Building:
                            request.BillingData.Building,

                        Floor:
                            request.BillingData.Floor,

                        Apartment:
                            request.BillingData.Apartment
                    ),

                SpecialReference:
                    request.Reference,

                Expiration:
                    _options
                        .IntentionExpirationSeconds,

                NotificationUrl:
                    request.PaymentMethod ==
                        PaymentMethod.Card
                            ? _options.NotificationUrl
                            : null,

                RedirectionUrl:
                    _options.RedirectionUrl
            );


        // ==========================================
        // HTTP Request
        // ==========================================

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "v1/intention/"
            );


        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Token",
                _options.SecretKey
            );


        httpRequest.Content =
            JsonContent.Create(
                apiRequest
            );


        try
        {
            using var response =
                await _httpClient.SendAsync(
                    httpRequest
                );


            if (!response.IsSuccessStatusCode)
            {
                var errorBody =
                    await response.Content
                        .ReadAsStringAsync();


                _logger.LogError(
                    "Paymob Create Intention failed. " +
                    "StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    errorBody
                );


                return Result
                    .Failure<CreatePaymobIntentionResponse>(
                        PaymobErrors.CreateIntentionFailed
                    );
            }


            var paymobResponse =
                await response.Content
                    .ReadFromJsonAsync<
                        PaymobCreateIntentionApiResponse
                    >();


            if (paymobResponse is null ||
                string.IsNullOrWhiteSpace(
                    paymobResponse.Id
                ) ||
                string.IsNullOrWhiteSpace(
                    paymobResponse.ClientSecret
                ))
            {
                return Result
                    .Failure<CreatePaymobIntentionResponse>(
                        PaymobErrors.InvalidResponse
                    );
            }


            // ==========================================
            // Unified Checkout URL
            // ==========================================

            var checkoutUrl =
                BuildCheckoutUrl(
                    paymobResponse.ClientSecret
                );


            var result =
                new CreatePaymobIntentionResponse(
                    IntentionId:
                        paymobResponse.Id,

                    PaymobOrderId:
                        paymobResponse
                            .IntentionOrderId
                            .ToString(),

                    ClientSecret:
                        paymobResponse
                            .ClientSecret,

                    CheckoutUrl:
                        checkoutUrl
                );


            return Result.Success(
                result
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HTTP error while creating Paymob intention."
            );


            return Result
                .Failure<CreatePaymobIntentionResponse>(
                    PaymobErrors.CreateIntentionFailed
                );
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Timeout while creating Paymob intention."
            );


            return Result
                .Failure<CreatePaymobIntentionResponse>(
                    PaymobErrors.CreateIntentionFailed
                );
        }
    }

    public async Task<Result> RefundAsync(long transactionId, decimal amount)
    {
        // ==========================================
        // Validate
        // ==========================================

        if (transactionId <= 0)
        {
            return Result.Failure(
                PaymobErrors.InvalidTransactionId
            );
        }


        if (amount <= 0)
        {
            return Result.Failure(
                PaymobErrors.InvalidAmount
            );
        }


        // ==========================================
        // Convert EGP -> Piastres
        // ==========================================

        var amountInCents =
            ConvertToCents(amount);


        var apiRequest =
            new PaymobRefundApiRequest(
                TransactionId:
                    transactionId,

                AmountCents:
                    amountInCents
            );


        // ==========================================
        // HTTP Request
        // ==========================================

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/acceptance/void_refund/refund"
            );


        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Token",
                _options.SecretKey
            );


        httpRequest.Content =
            JsonContent.Create(apiRequest);


        try
        {
            using var response =
                await _httpClient.SendAsync(
                    httpRequest
                );


            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content
                        .ReadAsStringAsync();


                _logger.LogError(
                    "Paymob Refund failed. " +
                    "TransactionId: {TransactionId}, " +
                    "StatusCode: {StatusCode}, " +
                    "Response: {Response}",
                    transactionId,
                    response.StatusCode,
                    responseBody
                );


                return Result.Failure(
                    PaymobErrors.RefundFailed
                );
            }


            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Unknown Paymob Refund status. " +
                "TransactionId: {TransactionId}",
                transactionId
            );


            /*
             * مهم:
             * مينفعش نقول Failed هنا.
             *
             * ممكن Paymob تكون استلمت الـRefund
             * بالفعل لكن الاتصال انقطع قبل ما
             * نستلم الـresponse.
             */
            return Result.Failure(
                PaymobErrors.RefundStatusUnknown
            );
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Paymob Refund request timed out. " +
                "TransactionId: {TransactionId}",
                transactionId
            );


            return Result.Failure(
                PaymobErrors.RefundStatusUnknown
            );
        }
    }



    private int? GetIntegrationId(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.Card =>
                _options.CardIntegrationId,

            PaymentMethod.MobileWallet
                when _options.WalletIntegrationId.HasValue =>
                    _options.WalletIntegrationId.Value,

            _ => null
        };
    }


    private static long ConvertToCents(decimal amount)
    {
        return decimal.ToInt64(
            decimal.Round(
                amount * 100m,
                0,
                MidpointRounding.AwayFromZero
            )
        );
    }


    private string BuildCheckoutUrl(string clientSecret)
    {
        var baseUrl =
            _options.CheckoutBaseUrl
                .TrimEnd('/');


        return
            $"{baseUrl}/" +
            $"?publicKey={Uri.EscapeDataString(_options.PublicKey)}" +
            $"&clientSecret={Uri.EscapeDataString(clientSecret)}";
    }
}