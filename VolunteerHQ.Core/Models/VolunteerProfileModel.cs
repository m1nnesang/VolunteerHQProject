namespace VolunteerHQ.Core.Models;

public class VolunteerProfileModel
{
    
    #region requiredFields
    public int Id { get; set; }
    public required string Bio { get; set; }
    public required string CvFilePath { get; set; }
    #endregion
    
    public string? Skills { get; set; }
    public string? Experience { get; set; }
    
    public int UserId { get; set; }
    public UserModel? User { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
}