namespace Omranco.BLL.DTOS.Payments.Paymob;

public sealed record PaymobBillingData(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Email,
    string City,
    string Country,
    string State,
    string Street,
    string Building,
    string Floor,
    string Apartment
);