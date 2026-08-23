using FluentValidation;

namespace FinancialPlanner.Application.Features.Aims.Commands.UpdateAim;

public class UpdateAimCommandValidator : AbstractValidator<UpdateAimCommand>
{
    public UpdateAimCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(255).When(x => x.Name is not null);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).When(x => x.Amount.HasValue);
    }
}
