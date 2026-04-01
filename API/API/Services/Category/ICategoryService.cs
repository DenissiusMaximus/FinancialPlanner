using API.Dtos;
using API.Inputs;

namespace API.Services.Category;

public interface ICategoryService
{
    Task<CategoryDto?> CreateCategory(CreateCategoryInput input);
    Task<bool> DeleteCategory(int id);
    Task<IReadOnlyCollection<CategoryDto>> GetCategories();
    Task<CategoryDto?> GetCategoryById(int id);
    Task<CategoryDto?> UpdateCategory(int id, UpdateCategoryInput input);
}
