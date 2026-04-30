using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.DonationDTOs;
using VolunteerHQ.Core.DTOs.FundraiserDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IFundraiserService
{
    Task<FundraiserResponseDto> CreateFundraiser(int unitId , CreateFundraiserDto dto , CancellationToken ct = default);
    Task<FundraiserResponseDto> GetFundraiser(int fundraiserId, CancellationToken ct = default);
    Task<PagedResponseDto<FundraiserResponseDto>> GetAllFundraisers(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<FundraiserAssignmentResponseDto> AssignOrganization(int fundraiserId, int userId, int orgId ,  CancellationToken ct = default);
    Task<DonationResponseDto> Donate(int? userId, string uniqCode, CreateDonationDto dto, CancellationToken ct = default);
    Task<DonationResponseDto> DirectDonate(int? userId, int fundraiserId, CreateDonationDto dto, CancellationToken ct = default);
}