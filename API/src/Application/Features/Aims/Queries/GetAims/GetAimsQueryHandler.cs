using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Pagination;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Services;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Aims.Queries.GetAims;

public class GetAimsQueryHandler(
    IValidator<GetAimsQuery> validator,
    IAimRepository aimRepository,
    IAimProgressCalculator progressCalculator,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<PaginatedResult<AimDto>>> HandleAsync(GetAimsQuery query, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
            return Result.Failure<PaginatedResult<AimDto>>(validationResult.ToValidationError());

        var aims = await aimRepository.GetAllAsync(currentUser.RequiredUserId, ct);

        var calculationResult = progressCalculator.Calculate(aims);
        if (calculationResult.IsFailure)
            return Result.Failure<PaginatedResult<AimDto>>(calculationResult.Error);

        var dtos = aims.Select(aim =>
        {
            var dto = mapper.Map<AimDto>(aim);
            AimProgressMapping.ApplyProgress(dto, aim, calculationResult.Value);
            return dto;
        });

        var filtered = dtos
            .FilterBySources(query.SourceIds)
            .FilterByClosed(query.ClosedOnly)
            .ApplySorting(query.SortBy, query.SortDescending)
            .ToList();

        var page = filtered.Skip(query.Offset).Take(query.Limit).ToList();

        return PaginatedResult<AimDto>.Create(page, filtered.Count, query.Offset, query.Limit);
    }
}
