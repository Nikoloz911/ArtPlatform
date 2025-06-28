using FluentValidation;
using PortfolioService.DTOs;

namespace PortfolioService.Validators;
public class AddPortfolioValidator : AbstractValidator<AddPortfolioDTO>
{
    public AddPortfolioValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Portfolio Title is required.")
            .MaximumLength(100).WithMessage("Portfolio name must not exceed 100 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Portfolio description must not exceed 500 characters.");
    }
}
