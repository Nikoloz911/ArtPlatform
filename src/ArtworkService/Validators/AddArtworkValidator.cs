using ArtworkService.DTOs;
using FluentValidation;

namespace ArtworkService.Validators;

public class AddArtworkValidator : AbstractValidator<AddArtworkDTO>
{
    public AddArtworkValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category ID must be a positive number.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.");
        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .WithMessage("Category is required.")
            .MaximumLength(50)
            .WithMessage("Category cannot exceed 50 characters.");
        RuleFor(x => x.ImageAdress).NotEmpty().WithMessage("Image address is required.");
    }
}
