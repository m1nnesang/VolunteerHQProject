using FluentValidation;
using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateOrganizationRequestDtoValidator : AbstractValidator<CreateOrganizationRequestDto>
{
    public CreateOrganizationRequestDtoValidator()
    {
        RuleFor(x => x.Bio).NotEmpty().WithMessage("Біографія не може бути порожньою");
        RuleFor(x => x.Skills).NotEmpty().WithMessage("Навички не можуть бути порожніми");
        RuleFor(x => x.Experience).NotEmpty().WithMessage("Досвід не може бути порожнім");
        RuleFor(x => x.CvFilePath).NotEmpty().WithMessage("Шлях до CV не може бути порожнім");
        RuleFor(x => x.ProposedName).NotEmpty().WithMessage("Назва організації не може бути порожньою");
        RuleFor(x => x.City).NotEmpty().WithMessage("Місто не може бути порожнім");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Опис не може бути порожнім");
    }
}