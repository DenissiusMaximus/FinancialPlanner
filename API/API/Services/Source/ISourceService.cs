using API.Dtos;
using API.Inputs;

namespace API.Services.Source;

public interface ISourceService
{
    Task<SourceDtoLookup?> CreateSource(CreateSourceInput createSourceDto);
    Task<SourceDtoLookup?> ArchiveSource(int sourceId);
    Task<SourceDtoLookup?> UnArchiveSource(int sourceId);
    Task<SourceDtoDetailed?> GetSourceById(int sourceId);
    Task<IReadOnlyCollection<SourceDtoLookup>> GetSources();
    Task<SourceDtoLookup?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto);
    Task<SourceSummaryDto> GetSourceSummary();
    Task<bool> DeleteSource(int sourceId);
}
