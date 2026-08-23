using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Users.Queries.IsEmailAvailable;

public class IsEmailAvailableQueryHandler(IUserRepository userRepository)
{
    public async Task<Result<bool>> HandleAsync(IsEmailAvailableQuery query, CancellationToken ct)
    {
        var exists = await userRepository.ExistsByEmailAsync(query.Email, ct);

        return !exists;
    }
}
