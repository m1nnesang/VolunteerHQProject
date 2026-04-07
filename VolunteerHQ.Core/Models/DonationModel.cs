namespace VolunteerHQ.Core.Models;

public class DonationModel
{
    public int Id { get; set; }
    
    public int FundraiserId { get; set; }
    public FundraiserModel? Fundraiser { get; set; }
    
    public int FundraiserAssignmentId { get; set; }
    public FundraiserAssignmentModel? FundraiserAssignment { get; set; }
    
    public int? UserId { get; set; }
    public UserModel? User { get; set; }
    
    public decimal Amount { get; set; }
    
    public bool IsAnonymous { get; set; }
    
    public DateTime CreatedAt { get; set; }
}