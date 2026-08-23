using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<IReadOnlyCollection<CategoryDto>>> HandleAsync(GetCategoriesQuery query, CancellationToken ct)
    {
        var categories = await categoryRepository.GetAllAsync(currentUser.RequiredUserId, ct);

        IReadOnlyCollection<CategoryDto> dtos = mapper.Map<List<CategoryDto>>(categories);

        return Result.Success(dtos);
    }
}
