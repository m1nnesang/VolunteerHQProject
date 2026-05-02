using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.CommentsDTOs;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class CommentService : ICommentService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    
    public CommentService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<CommentResponseDto> CreateComment(int userId, int fundraiserId, CreateCommentDto dto,
        CancellationToken ct = default)
    {
        await _vs.GetFundraiserOrThrow(fundraiserId, ct);
        await _vs.GetUserByIdOrThrow(userId, ct);

        var message = new CommentModel()
        {
            UserId = userId,
            FundraiserId = fundraiserId,
            Text = dto.Text,
            CreatedAt = DateTime.UtcNow
        };
        
        await _db.AddAsync(message, ct);
        await _db.SaveChangesAsync(ct);
        
        return new CommentResponseDto(message.Id, message.UserId, message.FundraiserId, message.Text, message.CreatedAt);
    }
    
    public async Task<PagedResponseDto<CommentResponseDto>> GetCommentsByFundraiser(int fundraiserId, PaginationDto pagination,
        CancellationToken ct = default)
    {
        await _vs.GetFundraiserOrThrow(fundraiserId, ct);
        
        var total = await _db.Comments.CountAsync(c => c.FundraiserId == fundraiserId, ct);

        var items = await _db.Comments
            .Where(c => c.FundraiserId == fundraiserId)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .AsNoTracking()
            .Select(c => new CommentResponseDto(c.Id, c.UserId, c.FundraiserId, c.Text, c.CreatedAt))
            .ToListAsync(ct);
        
        return new PagedResponseDto<CommentResponseDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task DeleteComment(int userId, int commentId, CancellationToken ct = default)
    {
        var comment = await _vs.GetCommentOrThrow(commentId, ct);

        if (comment.UserId != userId) throw new NotEnoughRightsException("You are not the author of this comment");
        
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync(ct);
    }
}