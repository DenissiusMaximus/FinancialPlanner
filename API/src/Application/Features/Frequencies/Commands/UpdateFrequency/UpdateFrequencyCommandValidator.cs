using FluentValidation;

namespace FinancialPlanner.Application.Features.Frequencies.Commands.UpdateFrequency;

public class UpdateFrequencyCommandValidator : AbstractValidator<UpdateFrequencyCommand>
{
    public UpdateFrequencyCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100);
    }
}
