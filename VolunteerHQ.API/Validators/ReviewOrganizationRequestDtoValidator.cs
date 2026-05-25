using FluentValidation;
using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

namespace VolunteerHQ.API.Validators;

public class ReviewOrganizationRequestDtoValidator : AbstractValidator<ReviewOrganizationRequestDto>
{
    public ReviewOrganizationRequestDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Невірний статус заявки");
    }
}