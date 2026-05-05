using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.NotificationDTOs;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    private readonly IEmailService _es;
    
    public NotificationService(AppDbContext db , ValidatorService vs , IEmailService es)
    {
        _db = db;
        _vs = vs;
        _es = es;
    }

    public async Task<PagedResponseDto<NotificationResponseDto>> GetNotifications(int userId, PaginationDto pagination,
        CancellationToken ct = default)
    {
        var total = await _db.Notifications.CountAsync(n => n.UserId == userId, ct);

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SentAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResponseDto<NotificationResponseDto>(
            notifications.Select(n => new NotificationResponseDto(n.Id, n.UserId, n.Text, n.Link, n.IsRead, n.SentAt)).ToList(),
            total, pagination.Page, pagination.PageSize);
    }

    public async Task SendNotification(int userId, string text, string link, CancellationToken ct = default)
    {
        var user = await _vs.GetUserByIdOrThrow(userId, ct);

        var notification = new NotificationModel()
        {
            UserId = userId,
            Text = text,
            Link = link,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };
        
        await _db.Notifications.AddAsync(notification, ct);
        await _db.SaveChangesAsync(ct);

        await _es.SendAsync(user.Email, text , link, ct);
    }
    
    public async Task MarkAsRead(int userId, int notificationId, CancellationToken ct = default)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null) throw new NotFoundException("Notification not found");
        
        notification.IsRead = true;
        
        await _db.SaveChangesAsync(ct);
    }
}