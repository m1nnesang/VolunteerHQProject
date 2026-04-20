using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.MembershipDTOs;
using VolunteerHQ.Core.DTOs.OrganizationDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Core.Exceptions;

namespace VolunteerHQ.Infrastructure.Services;

public class OrganizationService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;

    public OrganizationService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }
    
    public async Task<OrganizationResponseDto> GetOrganization(int orgId, CancellationToken ct = default)
    {
        var organization = await _vs.GetOrganizationOrThrow(orgId, ct);
            
        return new OrganizationResponseDto(organization.Id, organization.OrganizationName, organization.City, organization.Description, organization.CreatedAt);
    }

    public async Task<List<MembershipResponseDto>> GetOrganizationMembers(int orgId, CancellationToken ct = default)
    {
        var organization = await _vs.GetOrganizationOrThrow(orgId, ct);

        return await _db.OrganizationMemberships
            .Where(m => m.OrganizationId == orgId)
            .Select(m => new MembershipResponseDto(m.Id, m.OrganizationId, m.MemberRole, m.JoinedAt))
            .ToListAsync(ct);
    }

    public async Task RemoveMember(int orgId, int requesterId , int targetId, CancellationToken ct = default)
    {
        // checking who will delete
        var requester = await _vs.GetUserInOrganizationOrThrow(requesterId, orgId, ct);

        if (requester.MemberRole != OrganizationMemberRole.Leader &&
            requester.MemberRole != OrganizationMemberRole.Deputy)
        {
            throw new NotEnoughRightsException("You don't have rights to do this type of operations");
        }
        
        //checking who will be DELETED
        var target = await _vs.GetUserInOrganizationOrThrow(targetId, orgId, ct);

        _db.OrganizationMemberships.Remove(target);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateMemberRole(int orgId, int requesterId, int targetId , UpdateMemberRoleDto dto, CancellationToken ct = default)
    {
        
        var requester = await _vs.GetUserInOrganizationOrThrow(requesterId, orgId, ct);

        if (requester.MemberRole != OrganizationMemberRole.Leader)
        {
            throw new NotEnoughRightsException("You don't have rights to do this type of operations");
        }
        
        var target = await _vs.GetUserInOrganizationOrThrow(targetId, orgId, ct);

        target.MemberRole = dto.newRole;
        await _db.SaveChangesAsync(ct);
    }
}
