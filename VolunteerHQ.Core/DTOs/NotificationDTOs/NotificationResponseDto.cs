namespace VolunteerHQ.Core.DTOs.NotificationDTOs;

public record NotificationResponseDto(int Id, int UserId, string Text, string Link, bool IsRead, DateTime SentAt);