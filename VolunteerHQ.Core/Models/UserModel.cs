using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.Models;

public class UserModel
{
    public int Id { get; set; }
    public DateOnly? BirthDate { get; set; }

    #region RequiredFields
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string SecondName { get; set; }
    
    #endregion
    
    public string? City { get; set; }
    public string? AvatarPath { get; set; }
    
    public UserRoles Role { get; set; }
    public VolunteerProfileModel? VolunteerProfile { get; set; }
    
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    
    
    public DateTime? BannedAt { get; set; }
    public DateTime CreatedAt { get; set; }

}