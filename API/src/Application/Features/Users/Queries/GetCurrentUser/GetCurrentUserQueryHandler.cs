using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.Users.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(IUserRepository userRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<UserDto>> HandleAsync(GetCurrentUserQuery query, CancellationToken ct)
    {
        var userId = currentUser.RequiredUserId;

        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure<UserDto>(UserErrors.NotFound(userId));

        return mapper.Map<UserDto>(user);
    }
}
