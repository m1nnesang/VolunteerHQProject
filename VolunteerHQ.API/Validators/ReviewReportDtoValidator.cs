using FluentValidation;
using VolunteerHQ.Core.DTOs.ReportDTOs;

namespace VolunteerHQ.API.Validators;

public class ReviewReportDtoValidator : AbstractValidator<ReviewReportDto>
{
    public ReviewReportDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid report status");
        RuleFor(x => x.AdminComment).MaximumLength(1000).WithMessage("Admin comment cannot exceed 1000 characters");
    }
}
