namespace Omranco.BLL.DTOS.Auths.Register.Requests
{
    public record AddRegisterRequest(
        string Email,
        string Password,
        string Name,
         string NationalId,
        string Department
        );
}
