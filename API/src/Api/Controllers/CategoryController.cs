using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.Categories.Commands.CreateCategory;
using FinancialPlanner.Application.Features.Categories.Commands.DeleteCategory;
using FinancialPlanner.Application.Features.Categories.Commands.UpdateCategory;
using FinancialPlanner.Application.Features.Categories.Queries.GetCategories;
using FinancialPlanner.Application.Features.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Authorize]
[Route("api/Category")]
public class CategoryController(
    CreateCategoryCommandHandler createCategoryCommandHandler,
    UpdateCategoryCommandHandler updateCategoryCommandHandler,
    DeleteCategoryCommandHandler deleteCategoryCommandHandler,
    GetCategoriesQueryHandler getCategoriesQueryHandler,
    GetCategoryByIdQueryHandler getCategoryByIdQueryHandler) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await getCategoriesQueryHandler.HandleAsync(new GetCategoriesQuery(), ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id, CancellationToken ct)
    {
        var result = await getCategoryByIdQueryHandler.HandleAsync(new GetCategoryByIdQuery(id), ct);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await createCategoryCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryCommand command, CancellationToken ct)
    {
        var result = await updateCategoryCommandHandler.HandleAsync(command with { Id = id }, ct);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct)
    {
        var result = await deleteCategoryCommandHandler.HandleAsync(new DeleteCategoryCommand(id), ct);

        return HandleResult(result);
    }
}
