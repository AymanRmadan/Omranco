namespace Main_Structure.BLL.DTOS.Auths.Passwords.Requests;
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
    );


