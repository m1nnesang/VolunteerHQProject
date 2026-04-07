namespace VolunteerHQ.Core.Models;

public class PrivateMessageModel
{
    public int Id { get; set; }
    
    public int? SenderId { get; set; }
    public UserModel? UserSender { get; set; }
    
    public int? ReceiverId { get; set; }
    public UserModel? UserReceiver { get; set; }
    
    public required string Text { get; set; }
    
    public bool IsRead { get; set; }
    
    public DateTime SentAt { get; set; }
}