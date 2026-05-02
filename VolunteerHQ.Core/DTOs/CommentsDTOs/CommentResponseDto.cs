namespace VolunteerHQ.Core.DTOs.CommentsDTOs;

public record CommentResponseDto(int Id, int? UserId, int FundraiserId, string Text, DateTime CreatedAt);