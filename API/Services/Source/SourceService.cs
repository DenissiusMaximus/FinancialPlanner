using System;
using API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Source;

public class SourceService(AppDbContext context) : ISourceService
{
    public async Task<SourceDto?> GetSourceById(int sourceId, int userId)
    {
        var source = await context.Sources
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .Select(s => new SourceDto
            {
                Id = s.Id,
                Name = s.Name,
                Amount = s.Amount,
                UserId = s.UserId,
                CurrencyId = s.CurrencyId
            }).FirstOrDefaultAsync();

        return source;
    }

    public async Task<List<SourceDto>> GetSources(int userId)
    {
        var sources = await context.Sources
            .Where(s => s.UserId == userId)
            .Select(s => new SourceDto
            {
                Id = s.Id,
                Name = s.Name,
                Amount = s.Amount,
                UserId = s.UserId,
                CurrencyId = s.CurrencyId
            }).ToListAsync();

        return sources;
    }

    public async Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto, int userId)
    {
        var source = new Models.Source
        {
            Name = createSourceDto.Name,
            Amount = createSourceDto.Amount,
            UserId = userId,
            CurrencyId = createSourceDto.CurrencyId
        };

        var result = context.Sources.Add(source);

        if (await context.SaveChangesAsync() > 0)
            return new SourceDto
            {
                Id = result.Entity.Id,
                Name = result.Entity.Name,
                Amount = result.Entity.Amount,
                UserId = result.Entity.UserId,
                CurrencyId = result.Entity.CurrencyId
            };

        return null;
    }

    public async Task<bool> DeleteSource(int sourceId, int userId)
    {
        var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
            return false;

        context.Sources.Remove(source);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto, int userId)
    {
        var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
            return null;

        if (updateSourceDto.Name != null)
            source.Name = updateSourceDto.Name;

        if (updateSourceDto.CurrencyId != null)
            source.CurrencyId = updateSourceDto.CurrencyId.Value;

        if (await context.SaveChangesAsync() > 0)
            return new SourceDto
            {
                Id = source.Id,
                Name = source.Name,
                Amount = source.Amount,
                UserId = source.UserId,
                CurrencyId = source.CurrencyId
            };

        return null;
    }
}
