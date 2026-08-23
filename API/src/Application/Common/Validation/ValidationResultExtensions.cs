using FinancialPlanner.Domain.Common;
using FluentValidation.Results;

namespace FinancialPlanner.Application.Common.Validation;

public static class ValidationResultExtensions
{
    public static ValidationError ToValidationError(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage, ErrorType.Validation))
            .ToArray();

        return new ValidationError(errors);
    }
}
