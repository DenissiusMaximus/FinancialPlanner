using API.Models;
using API.Utils.Notification;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TransactionTypeService(AppDbContext context, NotificationContext notificationContext) : ITransactionTypeService
{
    public async Task<TransactionTypeDto?> GetTransactionType(int id)
    {
        var transactionType = await context.TransactionTypes.FindAsync(id);

        if (transactionType == null)
        {
            notificationContext.AddNotification("Transaction type not found.", ErrorType.NotFound);
            return null;
        }

        return new TransactionTypeDto
        {
            Id = transactionType.Id,
            Name = transactionType.Name
        };
    }

    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes()
    {
        var transactionTypes = await context.TransactionTypes.AsNoTracking().ToListAsync();
        
        return [.. transactionTypes.Select(t => new TransactionTypeDto
        {
            Id = t.Id,
            Name = t.Name
        })];
    }
}
