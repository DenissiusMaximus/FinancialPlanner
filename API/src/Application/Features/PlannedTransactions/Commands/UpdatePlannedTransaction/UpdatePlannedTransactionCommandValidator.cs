using FluentValidation;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.UpdatePlannedTransaction;

public class UpdatePlannedTransactionCommandValidator : AbstractValidator<UpdatePlannedTransactionCommand>
{
    public UpdatePlannedTransactionCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(555).When(x => x.Name is not null);
        RuleFor(x => x.Amount).GreaterThan(0).When(x => x.Amount.HasValue);
        RuleFor(x => x.CurrencyId).GreaterThan(0).When(x => x.CurrencyId.HasValue);
        RuleFor(x => x.SourceId).GreaterThan(0).When(x => x.SourceId.HasValue);
        RuleFor(x => x.TransactionTypeId).GreaterThan(0).When(x => x.TransactionTypeId.HasValue);
        RuleFor(x => x.FrequencyId).GreaterThan(0).When(x => x.FrequencyId.HasValue);
    }
}
