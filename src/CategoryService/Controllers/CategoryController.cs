using AutoMapper;
using CategoryService.Data;
using CategoryService.DTOs;
using CategoryService.Models;
using CategoryService.Validators;
using Contracts.Core;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<AddCategoryDTO> _addCategoryValidator;
    private readonly IValidator<UpdateCategoryDTO> _updateCategoryValidator;

    public CategoryController(
        DataContext context,
        IMapper mapper,
        IValidator<AddCategoryDTO> addCategoryValidator,
        IValidator<UpdateCategoryDTO> updateCategoryValidator
    )
    {
        _context = context;
        _mapper = mapper;
        _addCategoryValidator = addCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
    }

    /// GET ALL CATEGORIES   /// GET ALL CATEGORIES   /// GET ALL CATEGORIES   /// GET ALL CATEGORIES
    [HttpGet("")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        if (categories == null || categories.Count == 0)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = "No categories found.",
                    Data = null,
                }
            );
        }
        var categoryDTOs = _mapper.Map<List<GetCategoriesDTO>>(categories);
        return Ok(
            new ApiResponse<List<GetCategoriesDTO>>
            {
                StatusCode = 200,
                Message = "All categories retrieved successfully.",
                Data = categoryDTOs,
            }
        );
    }

    /// GET CATEGORY BY ID   /// GET CATEGORY BY ID   /// GET CATEGORY BY ID   /// GET CATEGORY BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = $"Category with ID {id} not found.",
                    Data = null,
                }
            );
        }
        var categoryDTO = _mapper.Map<GetCategoriesByIdDTO>(category);
        return Ok(
            new ApiResponse<GetCategoriesByIdDTO>
            {
                StatusCode = 200,
                Message = "Category retrieved successfully.",
                Data = categoryDTO,
            }
        );
    }

    /// DELETE CATEGORY BY ID   /// DELETE CATEGORY BY ID   /// DELETE CATEGORY BY ID   /// DELETE CATEGORY BY ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = $"Category with ID {id} not found.",
                    Data = null,
                }
            );
        }
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(
            new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Category deleted successfully.",
                Data = null,
            }
        );
    }

    /// ADD NEW CATEGORY   /// ADD NEW CATEGORY   /// ADD NEW CATEGORY   /// ADD NEW CATEGORY
    [HttpPost("")]
    [Authorize(Policy = "AdminOnly")] 
    public async Task<IActionResult> AddCategory([FromBody] AddCategoryDTO dto)
    {
        var validationResult = await _addCategoryValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(
                new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "Validation failed.",
                    Data = validationResult.Errors.Select(e => new
                    {
                        e.PropertyName,
                        e.ErrorMessage,
                    }),
                }
            );
        }

        var category = _mapper.Map<Category>(dto);
        category.CreationDate = DateTime.UtcNow;
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        var responseDto = _mapper.Map<AddCategoryResponseDTO>(category);
        return Ok(
            new ApiResponse<AddCategoryResponseDTO>
            {
                StatusCode = 200,
                Message = "Category created successfully.",
                Data = responseDto,
            }
        );
    }
    /// UPDATE CATEGORY BY ID   /// UPDATE CATEGORY BY ID   /// UPDATE CATEGORY BY ID   /// UPDATE CATEGORY BY ID
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDTO dto)
    {
        var validationResult = await _updateCategoryValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                StatusCode = 400,
                Message = "Validation failed.",
                Data = validationResult.Errors.Select(e => new
                {
                    e.PropertyName,
                    e.ErrorMessage,
                }),
            });
        }
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = $"Category with ID {id} not found.",
                Data = null,
            });
        }
        _mapper.Map(dto, category);
        await _context.SaveChangesAsync();
        var responseDto = _mapper.Map<UpdateCategoryResponseDTO>(category);
        return Ok(new ApiResponse<UpdateCategoryResponseDTO>
        {
            StatusCode = 200,
            Message = "Category updated successfully.",
            Data = responseDto,
        });
    }
}
