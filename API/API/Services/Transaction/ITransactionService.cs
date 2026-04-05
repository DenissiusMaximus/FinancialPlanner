using API.Dtos;
using API.Inputs;
using API.Models;

namespace API.Services.Transaction;

public interface ITransactionService
{
    Task<PaginatedResult<TransactionDto>> GetUsersTransactions(GetUserTransactionsInput input);
    Task<TransactionDto?> GetTransactionById(int id);
    Task<TransactionDto?> CreateTransaction(CreateTransactionInput transactionCreateDto);
    Task<TransactionDto?> UpdateTransaction(int id, UpdateTransactionInput transactionUpdateDto);
    Task<bool> DeleteTransaction(int id);
}
