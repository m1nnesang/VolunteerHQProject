using FluentValidation;
using VolunteerHQ.Core.DTOs.DonationDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateDonationDtoValidator : AbstractValidator<CreateDonationDto>
{
    public CreateDonationDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("Note cannot exceed 500 characters").When(x => x.Note != null);
    }
}
