using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(
    IValidator<UpdateCategoryCommand> validator,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IPatchMapper patchMapper,
    IMapper mapper)
{
    public async Task<Result<CategoryDto>> HandleAsync(UpdateCategoryCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<CategoryDto>(validationResult.ToValidationError());

        var category = await categoryRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (category is null)
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound(command.Id));

        patchMapper.PatchInto(command, category);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<CategoryDto>(category);
    }
}
