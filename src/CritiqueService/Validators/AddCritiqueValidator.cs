using CritiqueService.DTOs;
using FluentValidation;

namespace CritiqueService.Validators;
public class AddCritiqueValidator : AbstractValidator<AddCritiqueDTO>
{
    public AddCritiqueValidator()
    {
        RuleFor(x => x.ArtworkId)
            .NotEmpty().WithMessage("Artwork ID is required.")
            .GreaterThan(0).WithMessage("Artwork ID must be greater than 0.");
        RuleFor(x => x.Rating)
            .NotEmpty().WithMessage("Rating is required.")
            .InclusiveBetween(1, 10).WithMessage("Rating must be between 1 and 10.");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(500).WithMessage("Text cannot exceed 500 characters.");
    }
}
