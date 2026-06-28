using Main_Structure.BLL.DTOS.Auths.Emails.Requests;

namespace Main_Structure.BLL.DTOS.Auths.Emails.Validators
{
    public class AddResendConfirmationEmailRequestValidator : AbstractValidator<AddResendConfirmationEmailRequest>
    {
        public AddResendConfirmationEmailRequestValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
