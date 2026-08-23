using FluentValidation;

namespace FinancialPlanner.Application.Features.Aims.Commands.CreateAim;

public class CreateAimCommandValidator : AbstractValidator<CreateAimCommand>
{
    public CreateAimCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
