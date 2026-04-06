namespace VolunteerHQ.Core.Models;

public class MilitaryUnitModel
{
    public int Id { get; set; }
    
    #region requiredFields
    public required string UnitName { get; set; }
    public required string ContactPersonName { get; set; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    #endregion
    
    public bool IsNameHidden { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
}