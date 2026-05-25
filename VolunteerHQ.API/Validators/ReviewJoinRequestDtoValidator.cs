using FluentValidation;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;

namespace VolunteerHQ.API.Validators;

public class ReviewJoinRequestDtoValidator : AbstractValidator<ReviewJoinRequestDto>
{
    public ReviewJoinRequestDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Невірний статус заявки");
    }
}