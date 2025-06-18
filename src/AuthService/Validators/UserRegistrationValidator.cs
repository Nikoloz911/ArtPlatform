using AuthService.DTOs;
using FluentValidation;

namespace AuthService.Validators;
public class UserRegistrationValidator : AbstractValidator<RegisterUserDTO>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .Matches(@"^[^@\s]+@[^@\s]+\.(com|net|org|edu|gov)$")
            .WithMessage("Email must contain '@' and end with '.com', '.net', etc.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters long.")
            .MaximumLength(60).WithMessage("Password must not exceed 60 characters.")
            .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"\d+").WithMessage("Password must contain at least one number.");

        RuleFor(x => x.Biography)
            .MaximumLength(1000).WithMessage("Biography must not exceed 1000 characters.");
    }
}
