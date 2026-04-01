using API.Inputs;

namespace API.Domain.Calculator;

public interface IAimProgressCalculator
{
    Task<IReadOnlyCollection<AimDto>> CalculateAimProgress(List<AimDto> aims);
}
