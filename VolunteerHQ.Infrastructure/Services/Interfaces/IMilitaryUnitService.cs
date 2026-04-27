using VolunteerHQ.Core.DTOs.MilitaryDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IMilitaryUnitService
{
    Task<MilitaryUnitResponseDto> CreateUnit(RegisterMilitaryUnitDto dto, int adminId, CancellationToken ct = default);
    Task<string> Login(LogMilitaryUnitDto dto, CancellationToken ct = default);
    Task<MilitaryUnitResponseDto> GetUnit(int unitId, int? userId, CancellationToken ct = default);
}