using Main_Structure.BLL.DTOS.Auths.Logins.Request;

namespace Main_Structure.BLL.DTOS.Auths.Logins.Validations
{
    public class AddLoginRequestValidator : AbstractValidator<AddLoginRequest>
    {
        public AddLoginRequestValidator()
        {
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.Password).NotEmpty();


        }

    }
}
