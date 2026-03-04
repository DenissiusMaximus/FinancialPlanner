using System;
using API.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateSourceValidator : AbstractValidator<CreateSourceInput>
{
    public CreateSourceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");

        RuleFor(x => x.Amount)
            .GreaterThan(-1).WithMessage("Amount must be non negative number");
        
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required")
            .GreaterThan(0).WithMessage("CurrencyId must be greater than 0");
    }
}
