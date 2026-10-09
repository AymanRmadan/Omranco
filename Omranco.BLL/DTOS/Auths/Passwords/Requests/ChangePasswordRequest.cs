namespace Omranco.BLL.DTOS.Auths.Passwords.Requests;
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
    );


