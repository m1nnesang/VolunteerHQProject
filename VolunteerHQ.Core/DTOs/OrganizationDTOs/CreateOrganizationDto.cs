using System.ComponentModel.DataAnnotations;

namespace VolunteerHQ.Core.DTOs.OrganizationDTOs;

// change sting City, to Nova Poshta API
public record CreateOrganizationDto([Required] string Name , [Required] string City , [Required] string Description);