using VolunteerHQ.Core.DTOs.CommentsDTOs;
using VolunteerHQ.Core.DTOs.Common;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface ICommentService
{
   Task<CommentResponseDto> CreateComment(int userId, int fundraiserId, CreateCommentDto dto, CancellationToken ct);
   Task<PagedResponseDto<CommentResponseDto>> GetCommentsByFundraiser(int fundraiserId, PaginationDto pagination, CancellationToken ct);
   Task DeleteComment(int userId, int commentId, CancellationToken ct);
}