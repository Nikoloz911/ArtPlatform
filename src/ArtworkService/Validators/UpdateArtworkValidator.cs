using ArtworkService.DTOs;
using FluentValidation;

namespace ArtworkService.Validators;
public class UpdateArtworkValidator : AbstractValidator<UpdateArtworkDTO>
{
   public UpdateArtworkValidator()
   {
      RuleFor(x => x.Title)
         .NotEmpty().WithMessage("Title is required.")
         .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");
      RuleFor(x => x.CategoryName)
         .NotEmpty().WithMessage("Category is required.")
         .MaximumLength(50).WithMessage("Category must not exceed 50 characters.");
        RuleFor(x => x.ImageAdress)
           .NotEmpty().WithMessage("Image address is required.");
    }
}