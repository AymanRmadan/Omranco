using Omranco.BLL.DTOS.Auths.Logins.Request;

namespace Omranco.BLL.DTOS.Auths.Logins.Validations
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
