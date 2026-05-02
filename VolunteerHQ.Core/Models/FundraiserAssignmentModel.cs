namespace VolunteerHQ.Core.Models;

public class FundraiserAssignmentModel
{
    public int Id { get; set; }
    
    public int FundraiserId { get; set; }
    public FundraiserModel? Fundraiser { get; set; }
    
    public int OrganizationId { get; set; }
    public OrganizationModel? Organization { get; set; }
    
    public required string UniqueCode { get; set; }
    
    public DateTime TakenAt { get; set; }
}