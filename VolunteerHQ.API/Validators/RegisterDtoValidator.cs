using FluentValidation;
using VolunteerHQ.Core.DTOs.AuthDTOs;

namespace VolunteerHQ.API.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email не може бути порожнім").EmailAddress().WithMessage("Невірний формат email");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль не може бути порожнім").MinimumLength(6).WithMessage("Пароль має містити мінімум 6 символів");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ім'я не може бути порожнім").MaximumLength(50).WithMessage("Ім'я не може перевищувати 50 символів");
        RuleFor(x => x.SecondName).NotEmpty().WithMessage("Прізвище не може бути порожнім").MaximumLength(50).WithMessage("Прізвище не може перевищувати 50 символів");
        RuleFor(x => x.BirthDate).LessThan(DateOnly.FromDateTime(DateTime.Today)).WithMessage("Дата народження має бути в минулому");
    }
}