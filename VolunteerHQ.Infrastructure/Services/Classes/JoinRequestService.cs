using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;


namespace VolunteerHQ.Infrastructure.Services.Classes;

public class JoinRequestService : IJoinRequestService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs; // validation service!

    public JoinRequestService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }
    
    
    public async Task<JoinRequestResponseDto> CreateJoinRequest(int userId, int orgId, CreateJoinRequestDto dto, CancellationToken ct = default)
    {
        var request = new JoinRequestModel
        {
            UserId = userId,
            OrganizationId = orgId,
            Status = RequestStatus.Pending,
            Bio = dto.Bio,
            Skills = dto.Skills,
            Experience = dto.Experience,
            CvFilePath = dto.CvFilePath,
            Motivation = dto.Motivation,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewedByUserId = null
        };
        
        await _db.AddAsync(request, ct);
        await _db.SaveChangesAsync(ct);

        return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId, request.Status,
            request.CreatedAt , request.ReviewedAt , request.ReviewedByUserId );
    }

    public async Task<JoinRequestResponseDto> GetJoinRequest(int joinRequestId , int userId , int orgId, CancellationToken ct = default)
    {
        // validators
        await _vs.CanManageRequestsToOrg(userId, orgId, ct);
        var request = await _vs.GetRequestOrThrow(joinRequestId, ct);
        
        return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId, 
            request.Status, request.CreatedAt, request.ReviewedAt, request.ReviewedByUserId);
    }

    public async Task<List<JoinRequestResponseDto>> GetAllJoinRequests(int orgId, CancellationToken ct = default)
    {
        var organization = await _vs.GetOrganizationOrThrow(orgId, ct);

        return await _db.JoinRequests
            .Where(r => r.OrganizationId == orgId)
            .Select(r => new JoinRequestResponseDto(r.Id, r.UserId, r.OrganizationId,
                r.Status, r.CreatedAt, r.ReviewedAt, r.ReviewedByUserId))
            .ToListAsync(ct);
    }

    public async Task<JoinRequestResponseDto> ReviewJoinRequest( ReviewJoinRequestDto dto, int reviewerId, int orgId , int requestId , CancellationToken ct = default)
    {
       await _vs.CanManageRequestsToOrg(reviewerId, orgId, ct);

       var request = await _vs.GetRequestOrThrow(requestId, ct);

       request.Status = dto.Status;
       request.ReviewedAt = DateTime.UtcNow;
       request.ReviewedByUserId = reviewerId;

       if (dto.Status == RequestStatus.Approved)
       {
           
           if (request.UserId == null) throw new NotFoundException("User not found");

           var member = new OrganizationMembershipModel
           {
               UserId = request.UserId.Value,
               MemberRole = OrganizationMemberRole.Member,
               JoinedAt = DateTime.UtcNow
           };
           
           var user = await _vs.GetUserByIdOrThrow(request.UserId.Value, ct);
           
           user.Role = UserRoles.Volunteer;

           var organization = await _vs.GetOrganizationOrThrow(orgId, ct); 
           organization.Memberships.Add(member);
       }

       await _db.SaveChangesAsync(ct);
       
       return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId, request.Status,
           request.CreatedAt , request.ReviewedAt , request.ReviewedByUserId );
    }
}

