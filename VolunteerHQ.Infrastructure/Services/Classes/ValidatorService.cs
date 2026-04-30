using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.AuthDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;


namespace VolunteerHQ.Infrastructure.Services.Classes;


public class ValidatorService
{
    private readonly AppDbContext _db;

    public ValidatorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrganizationMembershipModel> GetUserInOrganizationOrThrow(int userId, int orgId,
        CancellationToken ct = default)
    {
        var user = await _db.OrganizationMemberships.FirstOrDefaultAsync(
            m => m.UserId == userId && m.OrganizationId == orgId, ct);

        if (user == null) throw new NotFoundException("You are not member of this Organization");
        return user;
    }

    public async Task<OrganizationModel> GetOrganizationOrThrow(int orgId, CancellationToken ct = default)
    {
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (org == null) throw new NotFoundException("Organization not found");
        return org;
    }

    public async Task<JoinRequestModel> GetRequestOrThrow(int joinRequestId, CancellationToken ct = default)
    {
        var request = await _db.JoinRequests.FirstOrDefaultAsync(r => r.Id == joinRequestId, ct);

        if (request == null) throw new NotFoundException("Request with this id isn't found");

        return request;
    }

    public async Task CanManageRequestsToOrg(int userId, int orgId, CancellationToken ct = default) // validation ONLY FOR APPROVE VOLUNTEER
    {
        var user = await GetUserInOrganizationOrThrow(userId, orgId, ct);

        if (user.MemberRole != OrganizationMemberRole.Leader &&
            user.MemberRole != OrganizationMemberRole.Deputy &&
            user.MemberRole != OrganizationMemberRole.Moderator)
        {
            throw new NotEnoughRightsException("You don't have enough rights for this operation");
        }
    }

    public async Task<OrganizationRequestModel> GetOrgRequestOrThrow(int createRequestId,
        CancellationToken ct = default)
    {
        var createRequest = await _db.OrganizationRequests.FirstOrDefaultAsync(r => r.Id == createRequestId, ct);

        if (createRequest == null) throw new NotFoundException("Request with this id isn't found");

        return createRequest;
    }


    public async Task<UserModel> GetUserByIdOrThrow(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) throw new NotFoundException("User not found");
        return user;
    }

    public async Task AdminOrThrow(int userId, CancellationToken ct = default)
    {
        var user = await GetUserByIdOrThrow(userId, ct);
        if (user.Role != UserRoles.Admin)
            throw new NotEnoughRightsException("You don't have enough rights for this operation");
    }

    public async Task<RefreshTokenModel> GetRefreshTokenOrThrow(string refreshToken)
    {
        var token = await _db.RefreshToken.FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null) throw new NotFoundException("Refresh token not found");
        return token;
    }

    public async Task<MilitaryUnitModel> GetUnitOrThrow(int unitId, CancellationToken ct = default)
    {
        var unit = await _db.MilitaryUnits.FirstOrDefaultAsync(u => u.Id == unitId, ct);
        if (unit == null) throw new NotFoundException("Unit not found");
        return unit;
    }
    
    public async Task<FundraiserModel> GetFundraiserOrThrow(int fundraiserId, CancellationToken ct = default)
    {
        var fundraiser = await _db.Fundraisers
            .Include(f => f.Assignments)
            .FirstOrDefaultAsync(f => f.Id == fundraiserId, ct);
            
            
        if (fundraiser == null) throw new NotFoundException("Fundraiser not found");
        return fundraiser;
    }
}
