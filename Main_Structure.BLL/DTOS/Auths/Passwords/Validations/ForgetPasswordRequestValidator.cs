using Main_Structure.BLL.DTOS.Auths.Passwords.Requests;

namespace Main_Structure.BLL.DTOS.Auths.Passwords.Validations
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
