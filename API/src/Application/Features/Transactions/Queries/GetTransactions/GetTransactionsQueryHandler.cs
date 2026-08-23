using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Pagination;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Transactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Repositories.Filters;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionsQueryHandler(
    IValidator<GetTransactionsQuery> validator,
    ITransactionRepository transactionRepository,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<PaginatedResult<TransactionDto>>> HandleAsync(GetTransactionsQuery query, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
            return Result.Failure<PaginatedResult<TransactionDto>>(validationResult.ToValidationError());

        var filter = new TransactionFilter(
            query.CategoryId,
            query.FromDate,
            query.ToDate,
            query.SortBy,
            query.SortDescending,
            query.Offset,
            query.Limit);

        var (items, totalCount) = await transactionRepository.GetPagedAsync(currentUser.RequiredUserId, filter, ct);

        var dtos = mapper.Map<List<TransactionDto>>(items);

        return PaginatedResult<TransactionDto>.Create(dtos, totalCount, query.Offset, query.Limit);
    }
}
