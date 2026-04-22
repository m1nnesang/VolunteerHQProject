using System.ComponentModel.DataAnnotations;

namespace VolunteerHQ.Core.DTOs.AuthDTOs;

public record RegisterDto (string Email, string Password, string FirstName, string SecondName, DateOnly? BirthDate );
    