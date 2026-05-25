using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.Common;
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
    private readonly ValidatorService _vs;
    private readonly INotificationService _ns;
    private readonly IAuditLogService _als;

    public OrganizationRequestService(AppDbContext db, ValidatorService vs, INotificationService ns, IAuditLogService als)
    {
        _db = db;
        _vs = vs;
        _ns = ns;
        _als = als;
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

    public async Task<PagedResponseDto<OrganizationRequestResponseDto>> GetAllRequests(int userId, int page = 1, int pageSize = 20 ,  CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(userId, ct);

        var total = await _db.OrganizationRequests.CountAsync(ct);
        
        var items = await _db.OrganizationRequests
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new OrganizationRequestResponseDto (r.Id , r.UserId , r.Bio , r.Experience , r.Skills, r.CvFilePath, r.ProposedName, r.City , r.Description, r.Status, r.ReviewedAt, r.ReviewedByUserId, r.AdminComment ))
                .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResponseDto<OrganizationRequestResponseDto>(items, total, page, pageSize);
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
        
        var text = dto.Status == RequestStatus.Approved
            ? "Вашу заявку на створення організації схвалено"
            : "Вашу заявку на створення організації відхилено";

        await _ns.SendNotification(request.UserId, text, "/organization", ct);

        await _als.Log(userId, dto.Status.ToString(), "OrganizationRequest", request.Id,
            $"OrganizationRequest {request.Id} marked as {dto.Status} by admin {userId}", ct);
        
        return new OrganizationRequestResponseDto(request.Id, request.UserId, request.Bio, request.Experience,
            request.Skills, request.CvFilePath, request.ProposedName, request.City,
            request.Description, request.Status, request.CreatedAt,
            request.ReviewedByUserId, request.AdminComment);
    }
}