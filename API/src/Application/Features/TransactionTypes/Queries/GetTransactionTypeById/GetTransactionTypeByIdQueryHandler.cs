using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.TransactionTypes.Queries.GetTransactionTypeById;

public class GetTransactionTypeByIdQueryHandler(ITransactionTypeRepository transactionTypeRepository, IMapper mapper)
{
    public async Task<Result<TransactionTypeDto>> HandleAsync(GetTransactionTypeByIdQuery query, CancellationToken ct)
    {
        var transactionType = await transactionTypeRepository.GetByIdAsync(query.Id, ct);

        if (transactionType is null)
            return Result.Failure<TransactionTypeDto>(TransactionTypeErrors.NotFound(query.Id));

        return mapper.Map<TransactionTypeDto>(transactionType);
    }
}
