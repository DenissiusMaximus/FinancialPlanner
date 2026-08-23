using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Services;

public interface IAimProgressCalculator
{
    Result<AimProgressCalculation> Calculate(IReadOnlyList<Aim> aims);
}
