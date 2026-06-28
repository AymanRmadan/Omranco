using Main_Structure.BLL.DTOS.Auths.Emails.Requests;

namespace Main_Structure.BLL.DTOS.Auths.Emails.Validators
{
    public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
    {
        public ConfirmEmailRequestValidator()
        {
            RuleFor(r => r.UserId)
                .NotEmpty();


            RuleFor(r => r.Code)
                .NotEmpty();

        }

    }
}