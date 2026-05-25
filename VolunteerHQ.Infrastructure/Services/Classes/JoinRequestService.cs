using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.Common;
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
    private readonly ValidatorService _vs;
    private readonly IAuditLogService _als;
    private readonly INotificationService _ns;

    public JoinRequestService(AppDbContext db, ValidatorService vs, INotificationService ns, IAuditLogService als)
    {
        _db = db;
        _vs = vs;
        _ns = ns;
        _als = als;
    }
    
    
    public async Task<JoinRequestResponseDto> CreateJoinRequest(int userId, int orgId, CreateJoinRequestDto dto, CancellationToken ct = default)
    {
        var alreadyMember = await _db.OrganizationMemberships
            .AnyAsync(m => m.UserId == userId && m.OrganizationId == orgId, ct);
        if (alreadyMember)
            throw new ConflictException("Ви вже є учасником цієї організації");

        var existingRequest = await _db.JoinRequests
            .AnyAsync(r => r.UserId == userId && r.OrganizationId == orgId && r.Status == RequestStatus.Pending, ct);
        if (existingRequest)
            throw new ConflictException("Ви вже маєте активну заявку до цієї організації");

        var request = new JoinRequestModel
        {
            UserId = userId,
            OrganizationId = orgId,
            Status = RequestStatus.Pending,
            Bio = dto.Bio,
            Skills = dto.Skills,
            Experience = dto.Experience,
            CvFilePath = dto.CvFilePath,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewedByUserId = null
        };
        
        await _db.AddAsync(request, ct);
        await _db.SaveChangesAsync(ct);

        return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId, request.Status,
            request.CreatedAt , request.ReviewedAt , request.ReviewedByUserId,
            null, null, request.Bio, request.Skills, request.Experience);
    }

    public async Task<JoinRequestResponseDto> GetJoinRequest(int joinRequestId , int userId , int orgId, CancellationToken ct = default)
    {
        // validators
        await _vs.CanManageRequestsToOrg(userId, orgId, ct);
        var request = await _vs.GetRequestOrThrow(joinRequestId, ct);

        return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId,
            request.Status, request.CreatedAt, request.ReviewedAt, request.ReviewedByUserId,
            request.User != null ? request.User.FirstName : null,
            request.User != null ? request.User.SecondName : null,
            request.Bio, request.Skills, request.Experience);
    }

    public async Task<PagedResponseDto<JoinRequestResponseDto>> GetAllJoinRequests(int orgId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var total = await _db.JoinRequests.CountAsync(r => r.OrganizationId == orgId, ct);

        var items = await _db.JoinRequests
            .Where(r => r.OrganizationId == orgId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(r => new JoinRequestResponseDto(r.Id, r.UserId, r.OrganizationId, r.Status, r.CreatedAt, r.ReviewedAt, r.ReviewedByUserId,
                r.User != null ? r.User.FirstName : null,
                r.User != null ? r.User.SecondName : null,
                r.Bio, r.Skills, r.Experience))
            .ToListAsync(ct);

        return new PagedResponseDto<JoinRequestResponseDto>(items, total, page, pageSize);
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
       
       var text = dto.Status == RequestStatus.Approved 
           ? "Вашу заявку до організації схвалено" 
           : "Вашу заявку до організації відхилено";

       await _ns.SendNotification(request.UserId!.Value, text, $"/organization/{orgId}", ct);

       await _als.Log(reviewerId, dto.Status.ToString(), "JoinRequest", request.Id,
           $"JoinRequest {request.Id} marked as {dto.Status} by user {reviewerId}", ct);
       
       return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId, request.Status,
           request.CreatedAt , request.ReviewedAt , request.ReviewedByUserId,
           null, null, request.Bio, request.Skills, request.Experience);
    }
}

