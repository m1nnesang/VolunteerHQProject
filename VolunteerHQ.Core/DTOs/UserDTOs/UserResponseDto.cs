using System.ComponentModel.DataAnnotations;
using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.UserDTOs;

public record UserResponseDto( int Id , [EmailAddress] string Email , string FirstName , string SecondName , DateOnly? BirthDate , UserRoles Role ); 
