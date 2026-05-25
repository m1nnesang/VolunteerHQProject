namespace VolunteerHQ.Core.DTOs.MessageDTOs;

public record CreatePrivateMessageDto(int ReceiverId, string Text);