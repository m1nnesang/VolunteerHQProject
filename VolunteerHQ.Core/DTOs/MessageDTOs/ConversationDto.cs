namespace VolunteerHQ.Core.DTOs.MessageDTOs;

public record ConversationDto(int OtherUserId, string LastMessage, DateTime LastMessageAt, int UnreadCount);
