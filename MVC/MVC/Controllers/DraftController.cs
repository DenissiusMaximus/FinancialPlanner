using API.Extensions;
using API.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class DraftController : Controller
{
    public IActionResult Index()
    {
        var draft = HttpContext.Session.GetJson<CreateTransactionInput>("DraftTransaction") ?? new CreateTransactionInput
        {
            Date = DateOnly.FromDateTime(DateTime.Today)
        };
        return View(draft);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveDraft(CreateTransactionInput draft)
    {
        HttpContext.Session.SetJson("DraftTransaction", draft);
        TempData["Message"] = "Draft saved in session.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult ClearDraft()
    {
        HttpContext.Session.Remove("DraftTransaction");
        TempData["Message"] = "Draft cleared.";
        return RedirectToAction("Index", "Home");
    }
}
