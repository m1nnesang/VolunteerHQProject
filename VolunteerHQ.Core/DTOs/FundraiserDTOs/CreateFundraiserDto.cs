using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.FundraiserDTOs;

public record CreateFundraiserDto(string Title , string Description , decimal TotalGoal , FundraiserImportance Importance, DateOnly Deadline);