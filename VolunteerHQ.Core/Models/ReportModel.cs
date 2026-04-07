using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.Models;

public class ReportModel
{
    public int Id { get; set; }
    
    public int? ReporterId { get; set; }
    public UserModel? ReporterUser { get; set; }
    
    public int? ReportedId { get; set; }
    public UserModel? ReportedUser { get; set; }
    
    public required string Reason { get; set; }
    
    public ReportCategory Category { get; set; }
    public ReportStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    public string? AdminComment { get; set; }
}