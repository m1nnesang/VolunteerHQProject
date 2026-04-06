using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.Models;

public class OrganizationMembershipModel
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public UserModel? User { get; set; }
    
    public int OrganizationId { get; set; }
    public OrganizationModel? Organization { get; set; }
    
    public OrganizationMemberRole MemberRole { get; set; }
    
    public DateTime JoinedAt { get; set; }
}