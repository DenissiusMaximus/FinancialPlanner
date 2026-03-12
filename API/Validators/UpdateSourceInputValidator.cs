using API.Dtos;
using FluentValidation;

namespace API.Validators;

public class UpdateSourceInputValidator : AbstractValidator<UpdateSourceInput>
{
    public UpdateSourceInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");
    }
}
