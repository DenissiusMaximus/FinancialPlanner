using System;
using API.Dtos;
using API.Inputs;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Category;

public class CategoryService(AppDbContext context) : ICategoryService
{
    public async Task<List<CategoryDto>> GetCategories(int userId)
    {
        var rawCategories = context.Categories.Where(c => c.UserId == userId);

        return rawCategories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            UserId = c.UserId
        }).ToList();
    }

    public async Task<CategoryDto?> GetCategoryById(int id, int userId)
    {
        var category = context.Categories.FirstOrDefault(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            return null;
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            UserId = category.UserId
        };
    }

    public async Task<bool> CreateCategory(CategoryInput input, int userId)
    {
        var category = new Models.Category
        {
            Name = input.Name,
            UserId = userId
        };

        await context.Categories.AddAsync(category);
        var result = await context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateCategory(int id, CategoryInput input, int userId)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return false;

        category.Name = input.Name;

        var result = await context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> DeleteCategory(int id, int userId)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return false;

        context.Categories.Remove(category);
        var result = await context.SaveChangesAsync();

        return result > 0;
    }
}
