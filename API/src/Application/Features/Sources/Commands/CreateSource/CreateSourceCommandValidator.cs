using FluentValidation;

namespace FinancialPlanner.Application.Features.Sources.Commands.CreateSource;

public class CreateSourceCommandValidator : AbstractValidator<CreateSourceCommand>
{
    public CreateSourceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(-1);
        RuleFor(x => x.CurrencyId).NotEmpty().GreaterThan(0);
    }
}
