namespace VolunteerHQ.Core.Models;

public class OrganizationModel
{
    public int Id { get; set; }
    
    #region requiredField
    public required string OrganizationName { get; set; }
    public required string City { get; set; }
    public required string Description { get; set; }
    #endregion
    
    public ICollection<OrganizationMembershipModel> Memberships { get; set; } 
        = new List<OrganizationMembershipModel>(); // navigation prop for 1 transaction in create
    public DateTime CreatedAt { get; set; }
    
}