using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.Transactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Transactions.Queries.GetTransactionById;

public class GetTransactionByIdQueryHandler(
    ITransactionRepository transactionRepository,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<TransactionDto>> HandleAsync(GetTransactionByIdQuery query, CancellationToken ct)
    {
        var transaction = await transactionRepository.GetByIdAsync(query.Id, currentUser.RequiredUserId, ct);
        if (transaction is null)
            return Result.Failure<TransactionDto>(TransactionErrors.NotFound(query.Id));

        return mapper.Map<TransactionDto>(transaction);
    }
}
