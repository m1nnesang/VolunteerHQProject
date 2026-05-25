using FluentValidation;
using VolunteerHQ.Core.DTOs.MessageDTOs;

namespace VolunteerHQ.API.Validators;

public class UpdatePrivateMessageDtoValidator : AbstractValidator<UpdatePrivateMessageDto>
{
    public UpdatePrivateMessageDtoValidator()
    {
        RuleFor(x => x.Text).NotEmpty().WithMessage("Message cannot be empty").MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters");
    }
}
