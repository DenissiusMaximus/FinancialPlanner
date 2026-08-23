using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Sources.Commands.CreateSource;

public class CreateSourceCommandHandler(
    IValidator<CreateSourceCommand> validator,
    ISourceRepository sourceRepository,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<SourceDtoLookup>> HandleAsync(CreateSourceCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<SourceDtoLookup>(validationResult.ToValidationError());

        var currency = await currencyRepository.GetByIdAsync(command.CurrencyId, ct);
        if (currency is null)
            return Result.Failure<SourceDtoLookup>(CurrencyErrors.NotFound(command.CurrencyId));

        var source = new Source
        {
            Name = command.Name,
            Amount = command.Amount,
            UserId = currentUser.RequiredUserId,
            CurrencyId = command.CurrencyId,
            IsArchived = false
        };

        var added = await sourceRepository.AddAsync(source, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var dto = mapper.Map<SourceDtoLookup>(added);
        dto.Currency = mapper.Map<CurrencyDto>(currency);

        return dto;
    }
}
