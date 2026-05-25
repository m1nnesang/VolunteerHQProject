using FluentValidation;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateJoinRequestDtoValidator : AbstractValidator<CreateJoinRequestDto>
{
    public CreateJoinRequestDtoValidator()
    {
        RuleFor(x => x.Bio).NotEmpty().WithMessage("Біографія не може бути порожньою");
        RuleFor(x => x.Skills).NotEmpty().WithMessage("Навички не можуть бути порожніми");
        RuleFor(x => x.Experience).NotEmpty().WithMessage("Досвід не може бути порожнім");
    }
}