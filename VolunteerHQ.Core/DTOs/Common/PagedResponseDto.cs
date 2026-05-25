namespace VolunteerHQ.Core.DTOs.Common;

public record PagedResponseDto<T>(List<T> Items, int TotalCount, int Page, int PageSize);