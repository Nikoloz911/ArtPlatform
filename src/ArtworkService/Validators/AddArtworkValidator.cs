using ArtworkService.DTOs;
using FluentValidation;

namespace ArtworkService.Validators;
public class AddArtworkValidator : AbstractValidator<AddArtworkDTO>
{
 public AddArtworkValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");
        RuleFor(x => x.ImageAdress)
            .NotEmpty().WithMessage("Image address is required.");
    }
}
