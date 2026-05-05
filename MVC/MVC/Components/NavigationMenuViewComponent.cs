using API.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace API.Components;

public class NavigationMenuViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;

    public NavigationMenuViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        ViewBag.SelectedCategory = HttpContext.Request.Query["category"].ToString();
        var categories = await _categoryService.GetCategories();
        return View(categories.Select(c => c.Name).OrderBy(x => x));
    }
}
