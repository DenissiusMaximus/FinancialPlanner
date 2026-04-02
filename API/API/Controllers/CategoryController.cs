using API.Dtos;
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
        return Ok(await categoryService.GetCategories());
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        return Ok(await categoryService.GetCategoryById(id));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryInput input)
    {
        return Ok(await categoryService.CreateCategory(input));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryInput input)
    {
        return Ok(await categoryService.UpdateCategory(id, input));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(int id)
    {
        return Ok(await categoryService.DeleteCategory(id));
    }
}