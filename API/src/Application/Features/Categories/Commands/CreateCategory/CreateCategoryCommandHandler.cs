using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using Mapster;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
    IValidator<CreateCategoryCommand> validator,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<CategoryDto>> HandleAsync(CreateCategoryCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<CategoryDto>(validationResult.ToValidationError());

        var category = command.Adapt<Category>();
        category.UserId = currentUser.RequiredUserId;

        var added = await categoryRepository.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<CategoryDto>(added);
    }
}
