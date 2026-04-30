using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.FundraiserDTOs;

public record FundraiserResponseDto(
    int Id,
    int MilitaryUnitId,
    string Title,
    string Description,
    decimal TotalGoal,
    decimal CurrentProgress,
    FundraiserImportance Importance,
    FundraiserStatus Status,
    List<FundraiserAssignmentResponseDto> Assignments,
    DateOnly Deadline,
    DateTime CreatedAt
);