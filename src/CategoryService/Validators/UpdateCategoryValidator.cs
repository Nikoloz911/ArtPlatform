using CategoryService.DTOs;
using FluentValidation;

namespace CategoryService.Validators;
public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDTO>
{
    public UpdateCategoryValidator()
    {
        RuleFor(c => c.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
        RuleFor(c => c.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        RuleFor(c => c.ImageURL)
         .NotEmpty().WithMessage("Image URL is required.");
    }
}