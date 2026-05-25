using FluentValidation;
using VolunteerHQ.Core.DTOs.AuthDTOs;

namespace VolunteerHQ.API.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email не може бути порожнім").EmailAddress().WithMessage("Невірний формат email");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль не може бути порожнім").MinimumLength(6).WithMessage("Пароль має містити мінімум 6 символів");
    }
}