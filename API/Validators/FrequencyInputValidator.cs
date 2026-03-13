using API.Inputs;
using API.Models;
using FluentValidation;

namespace API.Validators;

public class FrequencyInputValidator : AbstractValidator<FrequencyInput>
{
    public FrequencyInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");

        RuleFor(x => x.IntervalUnitId)
            .NotEmpty().WithMessage("IntervalUnitId is required")
            .GreaterThan(0).WithMessage("IntervalUnitId must be greater than 0");
        
        RuleFor(x => x.IntervalValue)
            .NotEmpty().WithMessage("IntervalValue is required")
            .GreaterThan(0).WithMessage("IntervalValue must be greater than 0");
    }
}