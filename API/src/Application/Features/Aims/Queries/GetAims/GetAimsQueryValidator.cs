using FluentValidation;

namespace FinancialPlanner.Application.Features.Aims.Queries.GetAims;

public class GetAimsQueryValidator : AbstractValidator<GetAimsQuery>
{
    public GetAimsQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}
