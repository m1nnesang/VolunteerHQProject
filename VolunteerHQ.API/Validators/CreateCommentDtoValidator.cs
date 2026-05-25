using FluentValidation;
using VolunteerHQ.Core.DTOs.CommentsDTOs;

namespace VolunteerHQ.API.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Text).NotEmpty().WithMessage("comment cannot be empty").MaximumLength(150);
    }
}