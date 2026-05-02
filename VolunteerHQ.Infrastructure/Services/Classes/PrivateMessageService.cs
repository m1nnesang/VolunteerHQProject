using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.MessageDTOs;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class PrivateMessageService : IPrivateMessageService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    
    public PrivateMessageService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<PrivateMessageResponseDto> SendMessage(int senderId,  CreatePrivateMessageDto dto, CancellationToken ct = default)
    {
        await _vs.GetUserByIdOrThrow(dto.ReceiverId, ct);

        var message = new PrivateMessageModel()
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Text = dto.Text,
            IsRead = false,
            IsEdited = false,
            SentAt = DateTime.UtcNow
        };
        
        _db.PrivateMessages.Add(message);
        await _db.SaveChangesAsync(ct);
        
        return new PrivateMessageResponseDto(message.Id, message.SenderId, message.ReceiverId, message.Text, message.IsRead, message.IsEdited, message.SentAt);
    }

    public async Task<PagedResponseDto<PrivateMessageResponseDto>> GetMessages(int userId, int otherUserId, PaginationDto pagination, CancellationToken ct = default)
    {
        var query = _db.PrivateMessages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == userId));
        
        var total = await query
            .CountAsync(ct);
        
        var items = await query
            .OrderBy(m => m.SentAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .AsNoTracking()
            .Select(m =>
                new PrivateMessageResponseDto(m.Id, m.SenderId, m.ReceiverId, m.Text, m.IsRead, m.IsEdited, m.SentAt))
            .ToListAsync(ct);

        return new PagedResponseDto<PrivateMessageResponseDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task DeleteMessage(int userId, int messageId, CancellationToken ct = default)
    {
        var message = await _vs.GetMessageOrThrow(messageId, ct);
        
        if (message.SenderId != userId) throw new NotEnoughRightsException("You are not the author of this message");
        
        _db.PrivateMessages.Remove(message);
        await _db.SaveChangesAsync(ct);
    }


    public async Task MarkAsRead(int userId, int messageId, CancellationToken ct = default)
    {
        var message = await _vs.GetMessageOrThrow(messageId, ct);
        
        if (message.ReceiverId != userId) throw new NotEnoughRightsException("You are not the receiver of this message");
        
        message.IsRead = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PrivateMessageResponseDto> UpdateMessage(int userId, int messageId, UpdatePrivateMessageDto dto,  CancellationToken ct = default)
    {
        var message = await _vs.GetMessageOrThrow(messageId, ct);
        
        if (message.SenderId != userId) throw new NotEnoughRightsException("You are not the author of this message");

        message.Text = dto.Text;
        message.IsEdited = true;
        
        await _db.SaveChangesAsync(ct);

        return new PrivateMessageResponseDto(message.Id, message.SenderId, message.ReceiverId, message.Text,
            message.IsRead, message.IsEdited, message.SentAt);
    }
}