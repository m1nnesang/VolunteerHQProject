namespace VolunteerHQ.Core.Models;

public class CommentModel
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public UserModel? User { get; set; }
    
    public int FundraiserId { get; set; }
    public FundraiserModel? Fundraiser { get; set; }

    public required string Text { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
