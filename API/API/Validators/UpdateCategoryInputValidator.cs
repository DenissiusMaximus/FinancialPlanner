using API.Inputs;
using FluentValidation;

namespace API.Validators;

public class UpdateCategoryInputValidator : AbstractValidator<UpdateCategoryInput>
{
    public UpdateCategoryInputValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");
    }
}
