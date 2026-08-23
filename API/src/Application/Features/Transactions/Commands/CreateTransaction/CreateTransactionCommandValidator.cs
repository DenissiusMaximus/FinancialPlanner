using FluentValidation;

namespace FinancialPlanner.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.SourceId).NotEmpty();
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.TransactionTypeId).NotEmpty();
    }
}
