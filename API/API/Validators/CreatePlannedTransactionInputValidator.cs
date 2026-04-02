using API.Models;
using FluentValidation;

namespace API.Validators;

public class CreatePlannedTransactionInputValidator : AbstractValidator<CreatePlannedTransactionInput>
{
    public CreatePlannedTransactionInputValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be non negative number");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required");

        RuleFor(x => x.SourceId)
            .NotEmpty().WithMessage("SourceId is required");

        RuleFor(x => x.TransactionTypeId)
            .NotEmpty().WithMessage("TransactionTypeId is required");

        RuleFor(x => x.FrequencyId)
            .NotEmpty().WithMessage("FrequencyId is required");

    }
}
