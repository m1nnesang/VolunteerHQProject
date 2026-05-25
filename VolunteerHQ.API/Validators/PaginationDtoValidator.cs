using FluentValidation;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

namespace VolunteerHQ.API.Validators;

public class PaginationDtoValidator : AbstractValidator<PaginationDto>
{
    public PaginationDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}