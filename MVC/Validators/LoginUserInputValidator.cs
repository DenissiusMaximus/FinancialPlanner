using System;
using API.Inputs;
using FluentValidation;

namespace API.Validators;

public class LoginUserInputValidator : AbstractValidator<LoginUserInput>
{
    public LoginUserInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
    }
}
