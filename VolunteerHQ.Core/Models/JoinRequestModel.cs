using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.Models;

public class JoinRequestModel
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public UserModel? User { get; set; }

    public int OrganizationId { get; set; }
    public OrganizationModel? Organization { get; set; }

    #region requiredFiels
    public required string Bio { get; set; }
    public required string Skills { get; set; }
    public required string CvFilePath { get; set; }
    
    public required string Experience { get; set; }
    #endregion
    
    public RequestStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    public int? ReviewedByUserId { get; set; }
    public UserModel? ReviewedByUser { get; set; }

}