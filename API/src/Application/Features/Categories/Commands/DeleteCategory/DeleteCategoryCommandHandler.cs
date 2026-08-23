using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound(command.Id));

        categoryRepository.Remove(category);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
