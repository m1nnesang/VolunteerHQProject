namespace VolunteerHQ.Core.DTOs.MessageDTOs;

public record PrivateMessageResponseDto(int Id , int? SenderId, int? ReceiverId, string Text, bool IsRead, bool IsEdited, DateTime SentAt );