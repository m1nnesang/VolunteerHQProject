using FluentValidation;
using VolunteerHQ.Core.DTOs.ReportDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.ReportedId).GreaterThan(0).WithMessage("ReportedId must be greater than 0");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason cannot be empty").MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
        RuleFor(x => x.Category).IsInEnum().WithMessage("Invalid report category");
    }
}
