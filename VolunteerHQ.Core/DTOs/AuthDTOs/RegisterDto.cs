using System.ComponentModel.DataAnnotations;

namespace VolunteerHQ.Core.DTOs.AuthDTOs;

public record RegisterDto (string Email , [MinLength(6)] string Password , string FirstName , string SecondName);