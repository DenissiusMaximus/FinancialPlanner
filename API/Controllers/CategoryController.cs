using API.Dtos;
using API.Extensions;
using API.Inputs;
using API.Services.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> Get()
    {
        var userId = User.GetRequiredUserId();
        
        return await categoryService.GetCategories(userId);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var userId = User.GetRequiredUserId();
        var category = await categoryService.GetCategoryById(id, userId);

        if (category == null)
            return NotFound();

        return category;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<bool>> Create(CategoryInput input)
    {
        var userId = User.GetRequiredUserId();

        return await categoryService.CreateCategory(input, userId);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> Update(int id, CategoryInput input)
    {
        var userId = User.GetRequiredUserId();

        return await categoryService.UpdateCategory(id, input, userId);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(int id)
    {
        var userId = User.GetRequiredUserId();

        return await categoryService.DeleteCategory(id, userId);
    }
}