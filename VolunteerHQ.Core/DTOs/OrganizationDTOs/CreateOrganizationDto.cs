using System.ComponentModel.DataAnnotations;

namespace VolunteerHQ.Core.DTOs.OrganizationDTOs;

public record CreateOrganizationDto([Required] string Name , [Required] string City , [Required] string Description);