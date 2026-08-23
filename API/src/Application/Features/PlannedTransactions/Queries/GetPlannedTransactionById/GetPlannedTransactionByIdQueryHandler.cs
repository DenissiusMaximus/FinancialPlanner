using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.PlannedTransactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactionById;

public class GetPlannedTransactionByIdQueryHandler(
    IPlannedTransactionRepository plannedTransactionRepository,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<PlannedTransactionDto>> HandleAsync(GetPlannedTransactionByIdQuery query, CancellationToken ct)
    {
        var plannedTransaction = await plannedTransactionRepository.GetByIdAsync(query.Id, currentUser.RequiredUserId, ct);
        if (plannedTransaction is null)
            return Result.Failure<PlannedTransactionDto>(PlannedTransactionErrors.NotFound(query.Id));

        return mapper.Map<PlannedTransactionDto>(plannedTransaction);
    }
}
