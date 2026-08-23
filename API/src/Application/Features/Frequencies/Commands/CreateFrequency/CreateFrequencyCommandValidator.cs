using FluentValidation;

namespace FinancialPlanner.Application.Features.Frequencies.Commands.CreateFrequency;

public class CreateFrequencyCommandValidator : AbstractValidator<CreateFrequencyCommand>
{
    public CreateFrequencyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IntervalUnitId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.IntervalValue).NotEmpty().GreaterThan(0);
    }
}
