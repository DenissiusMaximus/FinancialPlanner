using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.TransactionTypes.Queries.GetTransactionTypes;

public class GetTransactionTypesQueryHandler(ITransactionTypeRepository transactionTypeRepository, IMapper mapper)
{
    public async Task<Result<IReadOnlyCollection<TransactionTypeDto>>> HandleAsync(GetTransactionTypesQuery query, CancellationToken ct)
    {
        var transactionTypes = await transactionTypeRepository.GetAllAsync(ct);

        IReadOnlyCollection<TransactionTypeDto> dtos = mapper.Map<List<TransactionTypeDto>>(transactionTypes);

        return Result.Success(dtos);
    }
}
