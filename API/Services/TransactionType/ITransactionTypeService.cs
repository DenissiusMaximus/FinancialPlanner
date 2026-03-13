using API.Models;

namespace API.Services;

public interface ITransactionTypeService
{
    public Task<TransactionTypeDto?> GetTransactionType(int id);
    public Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypes();    
}