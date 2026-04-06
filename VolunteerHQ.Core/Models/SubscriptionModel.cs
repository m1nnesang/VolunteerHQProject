using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.Models;

public class SubscriptionModel
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public UserModel? User { get; set; }
    
    public SubscriptionTargetType Target { get; set; }
    
    public int TargetId { get; set; }
    
    public DateTime SubscribedAt { get; set; }
}