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
    [HttpPost]
    public async Task<ActionResult<bool>> Create(CategoryInput input)
    {
        var userId = User.GetRequiredUserId();

        return await categoryService.CreateCategory(input, userId);
    }
}