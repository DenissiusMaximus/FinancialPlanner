using System;
using API.Dtos;
using API.Services.Source;

namespace API.Services.Logging;

public class SourceLoggingService(ISourceService innerService, ILogger<SourceLoggingService> logger) : ISourceService
{
    public async Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto, int userId)
    {
        var result = await innerService.CreateSource(createSourceDto, userId);

        if (result != null)
            logger.LogInformation("Source {SourceId} created successfully for user {UserId}", result.Id, userId);
        else
            logger.LogWarning("Failed to create source {SourceName} for user {UserId}", createSourceDto.Name, userId);

        return result;
    }

    public async Task<bool> DeleteSource(int sourceId, int userId)
    {
        var result = await innerService.DeleteSource(sourceId, userId);

        if (result)
            logger.LogInformation("Source {SourceId} deleted successfully for user {UserId}", sourceId, userId);
        else
            logger.LogWarning("Failed to delete source {SourceId} for user {UserId}", sourceId, userId);

        return result;
    }

    public async Task<SourceDto?> GetSourceById(int sourceId, int userId) =>
        await innerService.GetSourceById(sourceId, userId);

    public async Task<List<SourceDto>> GetSources(int userId) =>
        await innerService.GetSources(userId);

    public async Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto, int userId)
    {
        var result = await innerService.UpdateSource(sourceId, updateSourceDto, userId);

        if (result != null)
            logger.LogInformation("Source {SourceId} updated successfully for user {UserId}", sourceId, userId);
        else
            logger.LogWarning("Failed to update source {SourceId} for user {UserId}", sourceId, userId);

        return result;
    }
}
