using System;
using API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Source;

public class SourceService(AppDbContext context) : ISourceService
{
        public async Task<SourceDto?> GetSourceById(int sourceId, int userId)
    {
        var source = await context.Sources
            .Include(s => s.Currency)
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .Select(s => CreateSourceDto(s))
            .FirstOrDefaultAsync();

        return source;
    }

    public async Task<List<SourceDto>> GetSources(int userId)
    {
        var result = await context.Sources
            .Include(s => s.Currency)
            .Where(s => s.UserId == userId && !s.IsArchived) 
            .Select(s => CreateSourceDto(s))
            .ToListAsync(); 

        return result;
    }

    public async Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto, int userId)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == createSourceDto.CurrencyId);

        if (currency == null)
            return null;

        var source = new Models.Source
        {
            Name = createSourceDto.Name,
            Amount = createSourceDto.Amount,
            UserId = userId,
            CurrencyId = createSourceDto.CurrencyId,
            IsArchived = false,
            Currency = currency
        };

        var result = context.Sources.Add(source);

        if (await context.SaveChangesAsync() > 0)
            return CreateSourceDto(result.Entity);

        return null;
    }

    public async Task<SourceDto?> ArchiveSource(int sourceId, int userId)
    {
        var source = await context.Sources.Include(s => s.Currency).FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
            return null;

        source.IsArchived = true;
        if (await context.SaveChangesAsync() > 0)
            return CreateSourceDto(source);
        
        return null;
    }

    public async Task<SourceDto?> UnArchiveSource(int sourceId, int userId)
    {
        var source = await context.Sources.Include(s => s.Currency).FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
            return null;

        source.IsArchived = false;
        if (await context.SaveChangesAsync() > 0)
            return CreateSourceDto(source);
        
        return null;
    }

    public async Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto, int userId)
    {
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
            return null;

        if (updateSourceDto.Name != null)
            source.Name = updateSourceDto.Name;

        if (await context.SaveChangesAsync() > 0)
            return CreateSourceDto(source);

        return null;
    }

    private static SourceDto CreateSourceDto(Models.Source source)
    {
        return new SourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Amount = source.Amount,
            UserId = source.UserId,
            CurrencyId = source.CurrencyId,
            CurrencyName = source.Currency.Name
        };
    }
}
