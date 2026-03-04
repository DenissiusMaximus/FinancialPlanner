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
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters long");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
        
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required")
            .GreaterThan(0).WithMessage("CurrencyId must be greater than 0");
    }
}
