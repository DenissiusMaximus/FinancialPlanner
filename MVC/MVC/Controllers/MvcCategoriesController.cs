using System.Linq;
using API.Models;
using API.Hubs;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers;

public class MvcCategoriesController : Controller
{
    private readonly ILabRepository _repository;
    private readonly IHubContext<CategoriesHub> _hubContext;

    public MvcCategoriesController(ILabRepository repository, IHubContext<CategoriesHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    public IActionResult Index() => View(_repository.Categories.ToList());

    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();
        var category = _repository.Categories.Include(c => c.SubCategories).FirstOrDefault(c => c.Id == id);
        if (category == null) return NotFound();
        return View(category);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (category.UserId == 0) category.UserId = 1; // Default to User 1
        ModelState.Remove("User");
        ModelState.Remove("PlannedTransactions");
        ModelState.Remove("Transactions");
        ModelState.Remove("SubCategories");

        if (ModelState.IsValid)
        {
            _repository.CreateCategory(category);
            await _hubContext.Clients.All.SendAsync("CategoriesChanged", $"Category '{category.Name}' was created.");
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();
        var category = _repository.Categories.FirstOrDefault(c => c.Id == id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();
        if (category.UserId == 0) category.UserId = 1;
        ModelState.Remove("User");
        ModelState.Remove("PlannedTransactions");
        ModelState.Remove("Transactions");
        ModelState.Remove("SubCategories");

        if (ModelState.IsValid)
        {
            var existing = _repository.Categories.FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                existing.Name = category.Name;
                _repository.SaveCategory(existing);
                await _hubContext.Clients.All.SendAsync("CategoriesChanged", $"Category '{existing.Name}' was updated.");
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var category = _repository.Categories.FirstOrDefault(c => c.Id == id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = _repository.Categories.FirstOrDefault(c => c.Id == id);
        if (category != null)
        {
            var categoryName = category.Name;
            _repository.DeleteCategory(category);
            await _hubContext.Clients.All.SendAsync("CategoriesChanged", $"Category '{categoryName}' was deleted.");
        }
        return RedirectToAction(nameof(Index));
    }
}
