using FluentValidation;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactions;

public class GetPlannedTransactionsQueryValidator : AbstractValidator<GetPlannedTransactionsQuery>
{
    public GetPlannedTransactionsQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}
