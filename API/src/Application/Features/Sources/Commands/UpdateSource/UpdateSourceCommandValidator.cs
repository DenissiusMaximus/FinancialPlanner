using FluentValidation;

namespace FinancialPlanner.Application.Features.Sources.Commands.UpdateSource;

public class UpdateSourceCommandValidator : AbstractValidator<UpdateSourceCommand>
{
    public UpdateSourceCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100);
    }
}
