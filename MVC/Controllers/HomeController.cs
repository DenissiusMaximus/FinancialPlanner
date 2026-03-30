using API.Services.Aim;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class HomeController(IAimService aimService) : Controller
{
    private readonly IAimService _aimService = aimService;

    public async Task<IActionResult> Index()
    {
        var aims = await _aimService.GetAims();
        return View(aims);
    }
}