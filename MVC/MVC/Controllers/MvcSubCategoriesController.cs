using System.Linq;
using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class MvcSubCategoriesController : Controller
{
    private readonly ILabRepository _repository;

    public MvcSubCategoriesController(ILabRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index() => View(_repository.SubCategories.Include(s => s.Category).ToList());

    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();
        var sub = _repository.SubCategories.Include(s => s.Category).FirstOrDefault(s => s.Id == id);
        if (sub == null) return NotFound();
        return View(sub);
    }

    public IActionResult Create()
    {
        ViewBag.CategoryId = new SelectList(_repository.Categories.ToList(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SubCategory subCategory)
    {
        ModelState.Remove("Category");
        if (ModelState.IsValid)
        {
            _repository.CreateSubCategory(subCategory);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CategoryId = new SelectList(_repository.Categories.ToList(), "Id", "Name", subCategory.CategoryId);
        return View(subCategory);
    }

    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();
        var sub = _repository.SubCategories.FirstOrDefault(s => s.Id == id);
        if (sub == null) return NotFound();
        ViewBag.CategoryId = new SelectList(_repository.Categories.ToList(), "Id", "Name", sub.CategoryId);
        return View(sub);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, SubCategory subCategory)
    {
        if (id != subCategory.Id) return NotFound();
        ModelState.Remove("Category");
        if (ModelState.IsValid)
        {
            var existing = _repository.SubCategories.FirstOrDefault(s => s.Id == id);
            if (existing != null)
            {
                existing.Name = subCategory.Name;
                existing.CategoryId = subCategory.CategoryId;
                _repository.SaveSubCategory(existing);
            }
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CategoryId = new SelectList(_repository.Categories.ToList(), "Id", "Name", subCategory.CategoryId);
        return View(subCategory);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var sub = _repository.SubCategories.Include(s => s.Category).FirstOrDefault(s => s.Id == id);
        if (sub == null) return NotFound();
        return View(sub);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var sub = _repository.SubCategories.FirstOrDefault(s => s.Id == id);
        if (sub != null) _repository.DeleteSubCategory(sub);
        return RedirectToAction(nameof(Index));
    }
}
