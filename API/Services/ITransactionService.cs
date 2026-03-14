using API.Inputs;
using API.Models;

namespace API.Services;

public interface ITransactionService
{
    Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions();
    Task<TransactionDto?> GetTransactionById(int id);
    Task<TransactionDto?> CreateTransaction(TransactionInput transactionCreateDto);
    Task<TransactionDto?> UpdateTransaction(int id, TransactionInput transactionUpdateDto);
    Task<bool> DeleteTransaction(int id);
}
