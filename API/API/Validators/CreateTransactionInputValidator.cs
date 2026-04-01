using API.Inputs;
using FluentValidation;

namespace API.Validators;

public class CreateTransactionInputValidator : AbstractValidator<CreateTransactionInput>
{
    public CreateTransactionInputValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be non negative number");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must be at most 1000 characters long");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required");


        RuleFor(x => x.SourceId)
            .NotEmpty().WithMessage("SourceId is required");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required");

        RuleFor(x => x.TransactionTypeId)
            .NotEmpty().WithMessage("TransactionTypeId is required");

    }
}
