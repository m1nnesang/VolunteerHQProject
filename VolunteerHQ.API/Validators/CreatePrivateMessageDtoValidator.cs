using FluentValidation;
using VolunteerHQ.Core.DTOs.MessageDTOs;

namespace VolunteerHQ.API.Validators;

public class CreatePrivateMessageDtoValidator : AbstractValidator<CreatePrivateMessageDto>
{
    public CreatePrivateMessageDtoValidator()
    {
        RuleFor(x => x.Text).NotEmpty().WithMessage("Message cannot be empty").MaximumLength(150);
        RuleFor(x => x.ReceiverId).GreaterThan(0);
    }
}