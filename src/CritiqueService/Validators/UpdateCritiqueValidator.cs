using CritiqueService.DTOs;
using FluentValidation;

namespace CritiqueService.Validators;
public class UpdateCritiqueValidator : AbstractValidator<UpdateCritiqueDTO>
{
    public UpdateCritiqueValidator()
    {
        RuleFor(x => x.Rating)
            .NotEmpty().WithMessage("Rating is required.")
            .InclusiveBetween(1, 10).WithMessage("Rating must be between 1 and 10.");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(500).WithMessage("Text cannot exceed 500 characters.");
    }
}