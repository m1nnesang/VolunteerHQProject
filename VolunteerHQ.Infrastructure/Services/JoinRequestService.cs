using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;

 

namespace VolunteerHQ.Infrastructure.Services;

public class JoinRequestService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;

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
        await _vs.CanManageRequests(userId, orgId, ct);
        var request = await _vs.GetRequestById(joinRequestId, ct);
        
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
}

