using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Pagination;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.PlannedTransactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Repositories.Filters;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactions;

public class GetPlannedTransactionsQueryHandler(
    IValidator<GetPlannedTransactionsQuery> validator,
    IPlannedTransactionRepository plannedTransactionRepository,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<PaginatedResult<PlannedTransactionDto>>> HandleAsync(GetPlannedTransactionsQuery query, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
            return Result.Failure<PaginatedResult<PlannedTransactionDto>>(validationResult.ToValidationError());

        var filter = new PlannedTransactionFilter(query.MinAmount, query.MaxAmount, query.SortDescending, query.Offset, query.Limit);

        var (items, totalCount) = await plannedTransactionRepository.GetPagedAsync(currentUser.RequiredUserId, filter, ct);

        var dtos = mapper.Map<List<PlannedTransactionDto>>(items);

        return PaginatedResult<PlannedTransactionDto>.Create(dtos, totalCount, query.Offset, query.Limit);
    }
}
