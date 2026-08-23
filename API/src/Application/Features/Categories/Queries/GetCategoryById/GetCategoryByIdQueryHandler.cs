using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<CategoryDto>> HandleAsync(GetCategoryByIdQuery query, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(query.Id, currentUser.RequiredUserId, ct);
        if (category is null)
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound(query.Id));

        return mapper.Map<CategoryDto>(category);
    }
}
