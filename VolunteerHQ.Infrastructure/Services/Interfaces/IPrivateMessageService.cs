using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.MessageDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IPrivateMessageService
{
    Task<List<ConversationDto>> GetConversations(int userId, CancellationToken ct = default);
    Task<PrivateMessageResponseDto> SendMessage(int senderId, CreatePrivateMessageDto dto, CancellationToken ct = default);
    Task<PagedResponseDto<PrivateMessageResponseDto>> GetMessages(int userId, int otherUserId, PaginationDto pagination, CancellationToken ct = default);
    Task DeleteMessage(int senderId, int messageId, CancellationToken ct = default);
    Task MarkAsRead(int userId, int messageId, CancellationToken ct);
    Task<PrivateMessageResponseDto> UpdateMessage(int senderId, int messageId, UpdatePrivateMessageDto dto, CancellationToken ct = default);
}