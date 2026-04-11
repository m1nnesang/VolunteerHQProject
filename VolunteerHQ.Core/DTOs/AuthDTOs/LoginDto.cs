using System.ComponentModel.DataAnnotations;

namespace VolunteerHQ.Core.DTOs.AuthDTOs;

public record LoginDto ([EmailAddress]string Email , string Password);