using API.Inputs;
using API.Models;
using API.Utils.Notification;
using API.Utils.UserContext;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface ITransactionService
{
    Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions();
    Task<TransactionDto?> GetTransactionById(int id);
    Task<TransactionDto?> CreateTransaction(TransactionInput transactionCreateDto);
    Task<TransactionDto?> UpdateTransaction(int id, TransactionInput transactionUpdateDto);
    Task<bool> DeleteTransaction(int id);
}

public class TransactionService(AppDbContext context, ICurrentUserContext currentUserContext, NotificationContext notificationContext) : ITransactionService
{
    public async Task<TransactionDto?> CreateTransaction(TransactionInput transactionCreateDto)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = new Transaction
        {
            Amount = transactionCreateDto.Amount,
            Comment = transactionCreateDto.Comment,
            Date = transactionCreateDto.Date,
            CategoryId = transactionCreateDto.CategoryId,
            SourceId = transactionCreateDto.SourceId,
            DestinationSourceId = transactionCreateDto.DestinationSourceId,
            CurrencyId = transactionCreateDto.CurrencyId,
            TransactionTypeId = transactionCreateDto.TransactionTypeId,
            UserId = userId
        };

        var addedTransaction = context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        return CreateTransactionDto(addedTransaction.Entity);
    }

    public async Task<bool> DeleteTransaction(int id)
    {
        var userId = currentUserContext.RequiredUserId;
        var transaction = context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return false;
        }

        context.Transactions.Remove(transaction);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<TransactionDto?> GetTransactionById(int id)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = await context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return null;
        }

        return CreateTransactionDto(transaction);
    }

    public async Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions()
    {
        var userId = currentUserContext.RequiredUserId;

        var transactions = await context.Transactions.AsNoTracking().Where(t => t.UserId == userId).Select(t => CreateTransactionDto(t)).ToListAsync();

        return transactions;
    }

    public async Task<TransactionDto?> UpdateTransaction(int id, TransactionInput transactionUpdateDto)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);
        
        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return null;
        }

        context.Entry(transaction).CurrentValues.SetValues(transactionUpdateDto);
        
        await context.SaveChangesAsync();

        return CreateTransactionDto(transaction);
    }

    private static TransactionDto CreateTransactionDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Comment = transaction.Comment,
            Date = transaction.Date,
            CategoryId = transaction.CategoryId,
            SourceId = transaction.SourceId,
            DestinationSourceId = transaction.DestinationSourceId,
            CurrencyId = transaction.CurrencyId,
            TransactionTypeId = transaction.TransactionTypeId
        };
    }
}
