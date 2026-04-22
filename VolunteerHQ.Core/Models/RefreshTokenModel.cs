namespace VolunteerHQ.Core.Models;

public class RefreshTokenModel
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}