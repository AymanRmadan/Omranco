using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Authentications.Requests;
using Omranco.BLL.DTOS.Auths.Emails.Requests;
using Omranco.BLL.DTOS.Auths.Logins.Request;
using Omranco.BLL.DTOS.Auths.Passwords.Requests;
using Omranco.BLL.DTOS.Auths.Register.Requests;
using Omranco.BLL.Services.Abstractions.AuthServices;

namespace Omranco.PL.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthsController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AddLoginRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);


            return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AddRegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authService.ChangePasswordAsync(User.GetUserId().ToString()!, request);

            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var result = await _authService.SendResetPasswordCodeAsync(request.Email);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] BLL.DTOS.Auths.Passwords.Requests.ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ConfirmEmailAsync(request);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] AddResendConfirmationEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResendConfirmationEmailAsync(request);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }



        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
        }

        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }


    }
}
