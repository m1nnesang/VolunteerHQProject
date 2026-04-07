using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.Models;

public class FundraiserModel
{
    public int Id { get; set; }
    
    public int MilitaryUnitId { get; set; }
    public MilitaryUnitModel? MilitaryUnit { get; set; }
    
    #region requiredField
    public required string Title { get; set; }
    public required string Description { get; set; }
    #endregion
    
    public  decimal TotalGoal { get; set; }
    public decimal CurrentProgress { get; set; }
    
    public FundraiserImportance Importance { get; set; }
    
    public DateOnly Deadline { get; set; }
    public FundraiserStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
}