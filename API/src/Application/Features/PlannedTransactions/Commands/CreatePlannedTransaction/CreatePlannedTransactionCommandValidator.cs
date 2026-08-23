using FluentValidation;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.CreatePlannedTransaction;

public class CreatePlannedTransactionCommandValidator : AbstractValidator<CreatePlannedTransactionCommand>
{
    public CreatePlannedTransactionCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.SourceId).NotEmpty();
        RuleFor(x => x.TransactionTypeId).NotEmpty();
        RuleFor(x => x.FrequencyId).NotEmpty();
    }
}
