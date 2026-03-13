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
        
        return Ok(await categoryService.GetCategories(userId));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await categoryService.GetCategoryById(id, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await categoryService.CreateCategory(input, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await categoryService.UpdateCategory(id, input, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(int id)
    {
        var userId = User.GetRequiredUserId();

        return Ok(await categoryService.DeleteCategory(id, userId));
    }
}