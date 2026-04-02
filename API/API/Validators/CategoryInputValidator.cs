using API.Inputs;
using FluentValidation;

namespace API.Validators;

public class CategoryInputValidator : AbstractValidator<CreateCategoryInput>
{
    public CategoryInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");
    }
}
