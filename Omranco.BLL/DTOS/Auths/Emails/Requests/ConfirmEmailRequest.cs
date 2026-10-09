namespace Omranco.BLL.DTOS.Auths.Emails.Requests
{
    public record ConfirmEmailRequest(string UserId, string Code);
}
