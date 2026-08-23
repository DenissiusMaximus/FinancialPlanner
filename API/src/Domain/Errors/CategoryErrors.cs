using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class CategoryErrors
{
    public static Error NotFound(int id) => new(
        "Categories.NotFound",
        $"Category with id '{id}' was not found.",
        ErrorType.NotFound);
}
