using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class AimErrors
{
    public static Error NotFound(int id) => new(
        "Aims.NotFound",
        $"Aim with id '{id}' was not found.",
        ErrorType.NotFound);

    public static Error SourceAlreadyLinked(int aimId, int sourceId) => new(
        "Aims.SourceAlreadyLinked",
        $"Source '{sourceId}' is already associated with aim '{aimId}'.",
        ErrorType.Conflict);

    public static Error SourceLinkNotFound(int aimId, int sourceId) => new(
        "Aims.SourceLinkNotFound",
        $"Source '{sourceId}' is not associated with aim '{aimId}'.",
        ErrorType.NotFound);

    public static Error CurrencyMissing(int id) => new(
        "Aims.CurrencyMissing",
        $"Aim with id '{id}' has no currency set.",
        ErrorType.Conflict);
}
