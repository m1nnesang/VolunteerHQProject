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
    private readonly MembershipValidatorService _vs;

    public JoinRequestService(AppDbContext db , MembershipValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }
    
    
    public async Task<JoinRequestResponseDto> CreateJoinRequest(int userId, int orgId, CreateJoinRequestDto dto)
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
        
        await _db.AddAsync(request);
        await _db.SaveChangesAsync();

        return new JoinRequestResponseDto(request.Id, request.UserId, request.OrganizationId, request.Status,
            request.CreatedAt , request.ReviewedAt , request.ReviewedByUserId );
    }

    public async Task<JoinRequestResponseDto> GetJoinRequest(int requesterId , int orgId)
    {
        var requester = await _db.OrganizationMemberships.FirstOrDefaultAsync(r => r.UserId == requesterId);

        if (requester.MemberRole != OrganizationMemberRole.Leader &&
            requester.MemberRole != OrganizationMemberRole.Deputy &&
            requester.MemberRole != OrganizationMemberRole.Moderator)
        {
            throw new NotEnoughRightsException("You don't have enough rights for this operation");
        }
    }
}

