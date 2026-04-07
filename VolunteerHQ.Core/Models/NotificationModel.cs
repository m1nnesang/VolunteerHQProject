namespace VolunteerHQ.Core.Models;

public class NotificationModel
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public UserModel? User { get; set; }
    
    public required string Text { get; set; }
    public required string Link { get; set; }
    
    public bool IsRead { get; set; }
    
    public DateTime SentAt { get; set; }
    
    
}