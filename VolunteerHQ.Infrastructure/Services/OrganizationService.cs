using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.MembershipDTOs;
using VolunteerHQ.Core.DTOs.OrganizationDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Core.Exceptions;

namespace VolunteerHQ.Infrastructure.Services;

public class OrganizationService
{
    private readonly AppDbContext _db;

    public OrganizationService(AppDbContext db)
    {
        _db = db;
    }
    
    private async Task<OrganizationMembershipModel> GetUserOrThrow(int userId , int orgId)
    {
        var user = await _db.OrganizationMemberships.FirstOrDefaultAsync(m => m.UserId == userId && m.OrganizationId == orgId);
        
        if (user == null) throw new NotFoundException("You are not member of this Organization");
        return user;
    }
    private async Task<OrganizationModel> GetOrganizationOrThrow(int orgId)
    {
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        
        if (org == null) throw new NotFoundException("Organization not found");
        return org;
    }

    public async Task<OrganizationResponseDto> CreateOrganization(CreateOrganizationDto dto, int userId)
    {
        var organization = new OrganizationModel
        {
            OrganizationName = dto.Name,
            City = dto.City,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        
        await _db.AddAsync(organization);
        
        var membership = new OrganizationMembershipModel
        {
            UserId = userId,
            OrganizationId = organization.Id,
            MemberRole = OrganizationMemberRole.Leader,
            JoinedAt = DateTime.UtcNow
        };
        
        
        await _db.AddAsync(membership);
        await _db.SaveChangesAsync();

        return new OrganizationResponseDto(organization.Id , dto.Name , dto.City , dto.Description , organization.CreatedAt );
    }

    public async Task<OrganizationResponseDto> GetOrganization(int orgId)
    {
        var organization = await GetOrganizationOrThrow(orgId);
            
        return new OrganizationResponseDto(organization.Id, organization.OrganizationName, organization.City, organization.Description, organization.CreatedAt);
    }

    public async Task<List<MembershipResponseDto>> GetOrganizationMembers(int orgId)
    {
        var organization = GetOrganizationOrThrow(orgId);

        return await _db.OrganizationMemberships
            .Where(m => m.OrganizationId == orgId)
            .Select(m => new MembershipResponseDto(m.Id, m.OrganizationId, m.MemberRole, m.JoinedAt))
            .ToListAsync();
    }

    public async Task RemoveMember(int orgId, int userId)
    {
        // checking who will delete
        var requester = await GetUserOrThrow(userId, orgId);

        if (requester.MemberRole != OrganizationMemberRole.Leader &&
            requester.MemberRole != OrganizationMemberRole.Deputy)
        {
            throw new NotEnoughRightsException("You don't have rights to do this type of operations");
        }
        
        //checking who will be DELETED
        var member = await GetUserOrThrow(userId, orgId);
        
        _db.OrganizationMemberships.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateMemberRole(int orgId, int userId, UpdateMemberRoleDto dto)
    {
        var requester = await GetUserOrThrow(userId, orgId);

        if (requester.MemberRole != OrganizationMemberRole.Leader)
        {
            throw new NotEnoughRightsException("You don't have rights to do this type of operations");
        }
        
        var member = await GetUserOrThrow(userId, orgId);

        member.MemberRole = dto.newRole;
        await _db.SaveChangesAsync();
        
    }
}
