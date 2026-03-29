using API.Inputs;
using FluentValidation;

namespace API.Validators;

public class UpdateTransactionInputValidator : AbstractValidator<UpdateTransactionInput>
{
    public UpdateTransactionInputValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(-1).WithMessage("Amount must be non negative number");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must be at most 1000 characters long");
    }
}
