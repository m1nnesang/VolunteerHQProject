namespace VolunteerHQ.Core.DTOs.CommentsDTOs;

public record CommentResponseDto(int Id, int? UserId, string? FirstName, string? SecondName, int FundraiserId, string Text, DateTime CreatedAt);