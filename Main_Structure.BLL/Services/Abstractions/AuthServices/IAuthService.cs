using Main_Structure.BLL.DTOS.Auths.Emails.Requests;
using Main_Structure.BLL.DTOS.Auths.Passwords.Requests;
using Main_Structure.BLL.DTOS.Auths.Register.Requests;

namespace Main_Structure.BLL.Services.Abstractions.AuthServices
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellation = default);
        Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellation = default);
        Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellation = default);

        Task<Result> RegisterAsync(AddRegisterRequest request, CancellationToken cancellationToken = default);

        Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task<Result> ResendConfirmationEmailAsync(AddResendConfirmationEmailRequest request);

        Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<Result> SendResetPasswordCodeAsync(string email);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    }

}
