using FinancialPlanner.Domain.Enums;
using FluentValidation;

namespace FinancialPlanner.Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(-1).When(x => x.Amount.HasValue);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.TransactionTypeId)
            .NotEqual((int)TransactionTypeEnum.Adjustment)
            .WithMessage("Adjustment transactions cannot be updated")
            .When(x => x.TransactionTypeId.HasValue);
    }
}
