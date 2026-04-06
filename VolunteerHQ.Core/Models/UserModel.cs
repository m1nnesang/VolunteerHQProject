using System.ComponentModel.DataAnnotations;

namespace VolunteerHQ.Core.Models;

public class User
{
    public int Id { get; init; }
    
    [Required]
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string? City { get; set; }
    public string AvatarPath { get; set; }
    public string Role 


}