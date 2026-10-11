using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Internal;

public sealed record PaymobBillingDataApiRequest(
    [property: JsonPropertyName("first_name")]
    string FirstName,

    [property: JsonPropertyName("last_name")]
    string LastName,

    [property: JsonPropertyName("phone_number")]
    string PhoneNumber,

    [property: JsonPropertyName("email")]
    string Email,

    [property: JsonPropertyName("city")]
    string City,

    [property: JsonPropertyName("country")]
    string Country,

    [property: JsonPropertyName("state")]
    string State,

    [property: JsonPropertyName("street")]
    string Street,

    [property: JsonPropertyName("building")]
    string Building,

    [property: JsonPropertyName("floor")]
    string Floor,

    [property: JsonPropertyName("apartment")]
    string Apartment
);