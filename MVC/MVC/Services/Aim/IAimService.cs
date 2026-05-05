using System;
using API.Dtos;
using API.Inputs;
using API.Models;

namespace API.Services.Aim;

public interface IAimService
{
    Task<AimDto?> GetAim(int id);
    Task<PaginatedResult<AimDto>> GetAims(GetAimsInput input);
    Task<AimDto?> CreateAim(CreateAimInput input);
    Task<AimDto?> UpdateAim(int id, UpdateAimInput input);
    Task<bool> DeleteAim(int id);
    Task<SourceDtoLookup?> AddSourceToAim(int aimId, int sourceId);
    Task<bool> RemoveSourceFromAim(int aimId, int sourceId);
}