namespace VolunteerHQ.Core.Models;

public class AuditLogModel
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public UserModel? User { get; set; }
    
    public required string Action { get; set; }
    
    public required string EntityType { get; set; }
    
    public int EntityId { get; set; }
    
    public required  string Details { get; set; }
    
    public DateTime CreatedAt { get; set; }
}