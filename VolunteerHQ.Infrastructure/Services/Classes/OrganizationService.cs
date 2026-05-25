using System.Xml;
using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.MembershipDTOs;
using VolunteerHQ.Core.DTOs.OrganizationDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;

    public OrganizationService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }
    
    public async Task<OrganizationResponseDto> GetOrganization(int orgId, int? requesterId = null, CancellationToken ct = default)
    {
        var organization = await _db.Organizations
            .Include(o => o.Memberships)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (organization == null)
            throw new NotFoundException("Organization not found");

        var isMember = requesterId != null &&
                       organization.Memberships.Any(m => m.UserId == requesterId.Value);

        var members = isMember
            ? organization.Memberships
                .Select(m => new MembershipResponseDto(m.Id, m.UserId, m.OrganizationId, m.MemberRole, m.JoinedAt, m.User != null ? m.User.FirstName : null, m.User != null ? m.User.SecondName : null))
                .ToList()
            : new List<MembershipResponseDto>();

        return new OrganizationResponseDto(organization.Id, organization.OrganizationName, organization.City, organization.Description, organization.CreatedAt, members);
    }

    public async Task<PagedResponseDto<OrganizationResponseDto>> GetAllOrganizations(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await _db.Organizations.CountAsync(ct);

        var items = await _db.Organizations
            .Include(o => o.Memberships)
                .ThenInclude(m => m.User)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        var dtos = items.Select(o => new OrganizationResponseDto(
            o.Id, o.OrganizationName, o.City, o.Description, o.CreatedAt,
            o.Memberships.Select(m => new MembershipResponseDto(m.Id, m.UserId, m.OrganizationId, m.MemberRole, m.JoinedAt, m.User != null ? m.User.FirstName : null, m.User != null ? m.User.SecondName : null)).ToList()
        )).ToList();

        return new PagedResponseDto<OrganizationResponseDto>(dtos, total, page, pageSize);
    }

    public async Task<int?> GetManagedOrgId(int userId, CancellationToken ct = default)
    {
        var managingRoles = new[]
        {
            OrganizationMemberRole.Leader,
            OrganizationMemberRole.Deputy
        };

        var membership = await _db.OrganizationMemberships
            .Where(m => m.UserId == userId && managingRoles.Contains(m.MemberRole))
            .Select(m => (int?)m.OrganizationId)
            .FirstOrDefaultAsync(ct);

        return membership;
    }

    public async Task<PagedResponseDto<MembershipResponseDto>> GetOrganizationMembers(int orgId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var total = await _db.OrganizationMemberships.CountAsync(m => m.OrganizationId == orgId, ct);

        var items = await _db.OrganizationMemberships
            .Where(m => m.OrganizationId == orgId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MembershipResponseDto(m.Id, m.UserId, m.OrganizationId, m.MemberRole, m.JoinedAt, m.User != null ? m.User.FirstName : null, m.User != null ? m.User.SecondName : null))
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResponseDto<MembershipResponseDto>(items, total, page, pageSize);
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
        
        if (requester.Id == targetId)
        {
            throw new ConflictException("You can't promote yourself");
        }

        if (dto.newRole == OrganizationMemberRole.Leader)
        {
            throw new ConflictException("Організація може мати лише одного лідера");
        }

        var target = await _vs.GetUserInOrganizationOrThrow(targetId, orgId, ct);

        target.MemberRole = dto.newRole;
        await _db.SaveChangesAsync(ct);
    }
}
