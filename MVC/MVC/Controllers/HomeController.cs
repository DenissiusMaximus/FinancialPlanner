using API.Models.ViewModels;
using API.Services;
using API.Services.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Application")]
public class HomeController : Controller
{
    private const int PageSize = 4;
    private readonly ITransactionService _transactionService;

    public HomeController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<IActionResult> Index(string? category, int productPage = 1)
    {
        var all = await _transactionService.GetUsersTransactions(new GetUserTransactionsInput { Limit = 1000 });
        var transactions = all.Data.AsEnumerable();

        if (!string.IsNullOrEmpty(category))
            transactions = transactions.Where(t => t.Category?.Name == category);

        var ordered = transactions.OrderByDescending(t => t.Date).ToList();
        var totalPages = (int)Math.Ceiling(ordered.Count / (double)PageSize);
        if (totalPages < 1) totalPages = 1;
        if (productPage < 1) productPage = 1;
        if (productPage > totalPages) productPage = totalPages;

        var paged = ordered.Skip((productPage - 1) * PageSize).Take(PageSize);

        var viewModel = new TransactionListViewModel
        {
            Transactions = paged,
            CurrentCategory = category,
            CurrentPage = productPage,
            TotalPages = totalPages
        };

        return View(viewModel);
    }
}
