using Omranco.BLL.DTOS.Auths.Passwords.Requests;

namespace Omranco.BLL.DTOS.Auths.Passwords.Validations
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .EmailAddress();


        }
    }
}
