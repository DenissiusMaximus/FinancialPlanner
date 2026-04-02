using API.Inputs;
using FluentValidation;

namespace API.Validators;

public class UpdateFrequencyInputValidator : AbstractValidator<UpdateFrequencyInput>
{
    public UpdateFrequencyInputValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");
    }
}
