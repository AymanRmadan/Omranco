using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Authentications.Responses;
using Omranco.BLL.DTOS.Auths.Emails.Requests;
using Omranco.BLL.DTOS.Auths.Passwords.Requests;
using Omranco.BLL.DTOS.Auths.Register.Requests;
using Omranco.BLL.Services.Abstractions.AuthServices;
using Omranco.Domain;
using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _refreshTokenExpiryDays = 14;
        public AuthService(UserManager<ApplicationUser> userManager
             , IJwtProvider jwtProvider
             , SignInManager<ApplicationUser> signInManager
             , ILogger<AuthService> logger
             , IEmailSender emailSender
             , IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);


            var result = await _signInManager.PasswordSignInAsync(user, password, false, true);

            if (result.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var (token, expiresIn) = _jwtProvider.GenerateToken(user, userRoles);
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiration
                });

                await _userManager.UpdateAsync(user);
                var response = new AuthResponse(user.Id.ToString(), user.Email, user.Name, token, expiresIn * 60, refreshToken, refreshTokenExpiration);
                return Result.Success(response);
            }

            var error = result.IsNotAllowed
            ? UserErrors.EmailNotConfirmed
            : result.IsLockedOut
            ? UserErrors.LockedUser
            : UserErrors.InvalidCredentials;

            return Result.Failure<AuthResponse>(error);

        }
        public async Task<Result> RegisterAsync(AddRegisterRequest request, CancellationToken cancellationToken = default)
        {
            var emailIsExist = await _userManager.Users.AnyAsync(user => user.Email == request.Email, cancellationToken);
            if (emailIsExist)
            {
                return Result.Failure<AuthResponse>(UserErrors.DuplicatedEmail);
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                Name = request.Name,
                UserName = request.Email,
                EmailConfirmed = true
            };


            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, DefaultRoles.Learner);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation("Confirmation code : {code}", code);
                await SendConfirmationEmail(user, code);

                return Result.Success();
            }

            var error = result.Errors.First();
            return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
                return Result.Failure(UserErrors.InvalidCode);

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            var code = request.Code;

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Result.Failure(UserErrors.InvalidCode);
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, DefaultRoles.Learner);
                return Result.Success();
            }

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }


        public async Task<Result> ResendConfirmationEmailAsync(AddResendConfirmationEmailRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Success();

            }

            if (user.EmailConfirmed)
            {
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            }
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Confirmation code : {code}", code);

            await SendConfirmationEmail(user, code);

            return Result.Success();
        }
        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellation = default)
        {
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);


            if (user.LockoutEnd > DateTime.UtcNow)
                return Result.Failure<AuthResponse>(UserErrors.LockedUser);

            var userRefrshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
            if (userRefrshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefrshToken.RevokedOn = DateTime.UtcNow;

            var userRoles = await _userManager.GetRolesAsync(user);
            var (newToken, expireIn) = _jwtProvider.GenerateToken(user, userRoles);

            var newRefreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration,

            });
            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id.ToString(), user.Email, user.Name
                                 , newToken, expireIn * 60
                                 , newRefreshToken, refreshTokenExpiration);

            return Result.Success(response);
        }



        public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellation = default)
        {
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            var userRefrshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
            if (userRefrshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefrshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            return Result.Success();
        }



        // This method when i have my password but i need to change it
        public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result.Failure(UserErrors.InvalidCredentials);

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }
        public async Task<Result> SendResetPasswordCodeAsync(string email)
        {
            // miss leading ==> to hacker
            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Success();

            if (!user.EmailConfirmed)
                return Result.Failure(UserErrors.EmailNotConfirmed with { StatusCode = StatusCodes.Status400BadRequest });

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Reset code: {code}", code);

            await SendResetPasswordEmail(user, code);

            return Result.Success();
        }



        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !user.EmailConfirmed)
                return Result.Failure(UserErrors.InvalidCode);

            IdentityResult result;

            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }


        private async Task SendResetPasswordEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            var emailBody = await EmailBodyBuilder.GenerateEmailBodyAsync("ForgetPassword",
                templateModel: new Dictionary<string, string>
                {
                { "{{name}}", user.Name },
                { "{{action_url}}", $"{origin}/auth/forgetPassword?email={user.Email}&code={code}" }
                }
            );

            await _emailSender.SendEmailAsync(user.Email!, "✅ General Template: Change Password", emailBody);

        }

        private async Task SendConfirmationEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            var emailBody = await EmailBodyBuilder.GenerateEmailBodyAsync("EmailConfirmation",
                templateModel: new Dictionary<string, string>
                {
                { "{{name}}", user.Name },
                    { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
                }
            );

            await _emailSender.SendEmailAsync(user.Email!, "✅ General Template: Email Confirmation", emailBody);
        }


        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }



    }
}
