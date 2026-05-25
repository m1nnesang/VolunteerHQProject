namespace VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

public record CreateOrganizationRequestDto(string Bio , string Experience , string Skills , string CvFilePath , string ProposedName , string City , string Description);