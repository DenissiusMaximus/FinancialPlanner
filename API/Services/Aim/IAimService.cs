using System;
using API.Inputs;
using API.Models;

namespace API.Services.Aim;

public interface IAimService
{
    Task<AimDto?> GetAim(int id);
    Task<IReadOnlyCollection<AimDto>> GetAims();
    Task<AimDto?> CreateAim(CreateAimInput input);
    Task<AimDto?> UpdateAim(int id, UpdateAimInput input);
    Task<bool> DeleteAim(int id);
}
