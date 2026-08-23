using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Services;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Aims.Queries.GetAim;

public class GetAimQueryHandler(
    IAimRepository aimRepository,
    IAimProgressCalculator progressCalculator,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<AimDto>> HandleAsync(GetAimQuery query, CancellationToken ct)
    {
        var aims = await aimRepository.GetAllAsync(currentUser.RequiredUserId, ct);

        var targetAim = aims.FirstOrDefault(a => a.Id == query.Id);
        if (targetAim is null)
            return Result.Failure<AimDto>(AimErrors.NotFound(query.Id));

        var calculationResult = progressCalculator.Calculate(aims);
        if (calculationResult.IsFailure)
            return Result.Failure<AimDto>(calculationResult.Error);

        var dto = mapper.Map<AimDto>(targetAim);
        AimProgressMapping.ApplyProgress(dto, targetAim, calculationResult.Value);

        return dto;
    }
}
