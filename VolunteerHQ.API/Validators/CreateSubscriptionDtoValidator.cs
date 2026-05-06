using FluentValidation;
using VolunteerHQ.Core.DTOs.SubscriptionDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateSubscriptionDtoValidator : AbstractValidator<CreateSubscriptionDto>
{
    public CreateSubscriptionDtoValidator()
    {
        RuleFor(x => x.TargetId).GreaterThan(0).WithMessage("TargetId must be greater than 0");
        RuleFor(x => x.Target).IsInEnum().WithMessage("Invalid subscription target type");
    }
}
