using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.SubscriptionDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    
    public SubscriptionService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<SubscriptionResponseDto> Subscribe(int userId, CreateSubscriptionDto dto, CancellationToken ct)
    {
        if (dto.Target == SubscriptionTargetType.Organization)
            await _vs.GetOrganizationOrThrow(dto.TargetId, ct);
        else
        {
           await _vs.GetUnitOrThrow(dto.TargetId, ct);
        }

        var existing = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Target == dto.Target && s.TargetId == dto.TargetId, ct);

        if (existing != null) throw new ConflictException("You already subscribed to this");

        var subscribe = new SubscriptionModel()
        {
            UserId = userId,
            Target = dto.Target,
            TargetId = dto.TargetId,
            SubscribedAt = DateTime.UtcNow
        };

        await _db.AddAsync(subscribe, ct);
        await _db.SaveChangesAsync(ct);

        var targetName = await GetTargetName(subscribe.Target, subscribe.TargetId, ct);

        return new SubscriptionResponseDto(subscribe.Id, subscribe.UserId, subscribe.Target, subscribe.TargetId, targetName, subscribe.SubscribedAt);
    }

    private async Task<string?> GetTargetName(SubscriptionTargetType target, int targetId, CancellationToken ct)
    {
        if (target == SubscriptionTargetType.Organization)
            return await _db.Organizations
                .Where(o => o.Id == targetId)
                .Select(o => o.OrganizationName)
                .FirstOrDefaultAsync(ct);

        return await _db.MilitaryUnits
            .Where(u => u.Id == targetId)
            .Select(u => u.IsNameHidden ? "********" : u.UnitName)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task Unsubscribe(int userId, int subscriptionId, CancellationToken ct)
    {
        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);
        
        if (subscription == null) throw new NotFoundException("Subscription not found");
        
        if (subscription.UserId != userId) throw new NotEnoughRightsException("You are not the owner of this subscription");
        
        _db.Subscriptions.Remove(subscription);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponseDto<SubscriptionResponseDto>> GetSubscriptions(int userId, PaginationDto dto,
        CancellationToken ct = default)
    {
        var total = await _db.Subscriptions.CountAsync(s => s.UserId == userId, ct);

        var subscriptions = await _db.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        var orgIds = subscriptions.Where(s => s.Target == SubscriptionTargetType.Organization).Select(s => s.TargetId).ToList();
        var unitIds = subscriptions.Where(s => s.Target == SubscriptionTargetType.MilitaryUnit).Select(s => s.TargetId).ToList();

        var orgNames = await _db.Organizations
            .Where(o => orgIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, o => o.OrganizationName, ct);

        var unitNames = await _db.MilitaryUnits
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.IsNameHidden ? "********" : u.UnitName, ct);

        var items = subscriptions.Select(s => new SubscriptionResponseDto(
            s.Id, s.UserId, s.Target, s.TargetId,
            s.Target == SubscriptionTargetType.Organization
                ? orgNames.GetValueOrDefault(s.TargetId)
                : unitNames.GetValueOrDefault(s.TargetId),
            s.SubscribedAt
        )).ToList();

        return new PagedResponseDto<SubscriptionResponseDto>(items, total, dto.Page, dto.PageSize);
    }
}
