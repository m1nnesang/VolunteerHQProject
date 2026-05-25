using FluentValidation;
using VolunteerHQ.Core.DTOs.FundraiserDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateFundraiserDtoValidator : AbstractValidator<CreateFundraiserDto>
{
    public CreateFundraiserDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title cannot be empty").MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description cannot be empty").MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
        RuleFor(x => x.TotalGoal).GreaterThan(0).WithMessage("Total goal must be greater than 0");
        RuleFor(x => x.Importance).IsInEnum().WithMessage("Invalid importance value");
        RuleFor(x => x.Deadline).GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Deadline must be in the future");
    }
}
