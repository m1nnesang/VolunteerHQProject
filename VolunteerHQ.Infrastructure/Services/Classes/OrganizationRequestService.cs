using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class OrganizationRequestService : IOrganizationRequestService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs; // validation service!

    public OrganizationRequestService(AppDbContext db, ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<OrganizationRequestResponseDto> CreateRequest(int userId, CreateOrganizationRequestDto dto,
        CancellationToken ct = default)
    {
        var request = new OrganizationRequestModel
        {
            UserId = userId,
            Bio = dto.Bio,
            Experience = dto.Experience,
            Skills = dto.Skills,
            CvFilePath = dto.CvFilePath,
            ProposedName = dto.ProposedName,
            City = dto.City,
            Description = dto.Description,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        await _db.AddAsync(request, ct);

        await _db.SaveChangesAsync(ct);

        return new OrganizationRequestResponseDto(request.Id, request.UserId, request.Bio, request.Experience,
            request.Skills, request.CvFilePath, request.ProposedName, request.City,
            request.Description, request.Status, request.CreatedAt,
            request.ReviewedByUserId, request.AdminComment);
    }

    public async Task<OrganizationRequestResponseDto> GetCreateRequest(int userId, int requestId,
        CancellationToken ct = default)
    {
        var request = await _vs.GetOrgRequestOrThrow(requestId, ct);

        var user = await _vs.GetUserByIdOrThrow(userId, ct);

        if (request.UserId != userId && user.Role != UserRoles.Admin)
            throw new NotEnoughRightsException("You don't have rights for this operation");

        return new OrganizationRequestResponseDto(request.Id, request.UserId, request.Bio, request.Experience,
            request.Skills, request.CvFilePath, request.ProposedName, request.City,
            request.Description, request.Status, request.CreatedAt,
            request.ReviewedByUserId, request.AdminComment);
    }

    public async Task<List<OrganizationRequestResponseDto>> GetAllRequests(int userId, CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(userId, ct);

        return await _db.OrganizationRequests
            .Select(o => new OrganizationRequestResponseDto(o.Id, o.UserId, o.Bio, o.Experience,
                o.Skills, o.CvFilePath, o.ProposedName, o.City,
                o.Description, o.Status, o.CreatedAt,
                o.ReviewedByUserId, o.AdminComment))
            .ToListAsync(ct);
    }

    public async Task<OrganizationRequestResponseDto> ReviewOrganizationRequest(int userId, int requestId,
        ReviewOrganizationRequestDto dto, CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(userId, ct);

        var request = await _vs.GetOrgRequestOrThrow(requestId, ct);

        request.Status = dto.Status;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedByUserId = userId;

        if (dto.Status == RequestStatus.Approved)
        {
            var organization = new OrganizationModel
            {
                OrganizationName = request.ProposedName,
                City = request.City,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            var member = new OrganizationMembershipModel
            {
                UserId = request.UserId,
                MemberRole = OrganizationMemberRole.Leader,
                JoinedAt = DateTime.UtcNow
            };

            var user = await _vs.GetUserByIdOrThrow(request.UserId, ct);

            user.Role = UserRoles.Volunteer;
            
            organization.Memberships.Add(member);
            await _db.AddAsync(organization, ct);
        }
        
        await _db.SaveChangesAsync(ct);

        return new OrganizationRequestResponseDto(request.Id, request.UserId, request.Bio, request.Experience,
            request.Skills, request.CvFilePath, request.ProposedName, request.City,
            request.Description, request.Status, request.CreatedAt,
            request.ReviewedByUserId, request.AdminComment);
    }
}