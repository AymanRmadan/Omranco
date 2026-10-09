namespace Omranco.BLL.DTOS.Auths.Passwords.Requests;

public record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);