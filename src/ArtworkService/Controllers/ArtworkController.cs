using ArtworkService.Data;
using ArtworkService.DTOs;
using ArtworkService.Models;
using AutoMapper;
using Contracts.Core;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtworkService.Controllers;

[Route("api/artworks")]
[ApiController]
public class ArtworkController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<AddArtworkDTO> _addArtworkValidator;
    private readonly IValidator<UpdateArtworkDTO> _updateValidator;

    public ArtworkController(DataContext context, IMapper mapper, IValidator<AddArtworkDTO> addArtworkValidator, IValidator<UpdateArtworkDTO> updateValidator)
    {
        _context = context;
        _mapper = mapper;
        _addArtworkValidator = addArtworkValidator;
        _updateValidator = updateValidator;
    }

    /// GET ALL ARTWORKS WITH FILTERS     /// GET ALL ARTWORKS WITH FILTERS      /// GET ALL ARTWORKS WITH FILTERS
    [HttpGet("")]
    public async Task<IActionResult> GetArtworks([FromQuery] GetArtworksFilterDTO filter)
    {
        var query = _context.Artwork.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            query = query.Where(a => a.Title.Contains(filter.Title));
        }
        if (!string.IsNullOrWhiteSpace(filter.CategoryName))
        {
            query = query.Where(a => a.CategoryName == filter.CategoryName);
        }
        if (filter.CreationTime.HasValue)
        {
            var creationDate = filter.CreationTime.Value.Date;
            query = query.Where(a => a.CreationTime.Date == creationDate);
        }
        var artworks = await query.OrderByDescending(a => a.CreationTime).ToListAsync();

        if (artworks.Count == 0)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = "No artworks found",
                    Data = null,
                }
            );
        }
        var response = _mapper.Map<List<ArtworkResponseDTO>>(artworks);
        return Ok(
            new ApiResponse<List<ArtworkResponseDTO>>
            {
                StatusCode = 200,
                Message = "Filtered artwork list retrieved.",
                Data = response,
            }
        );
    }

    /// GET ARTWORK BY ID    /// GET ARTWORK BY ID    /// GET ARTWORK BY ID    /// GET ARTWORK BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtworkById(int id)
    {
        var artwork = await _context.Artwork.FindAsync(id);
        if (artwork == null)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = $"Artwork with ID {id} not found.",
                    Data = null,
                }
            );
        }
        return Ok(
            new ApiResponse<Artwork>
            {
                StatusCode = 200,
                Message = "Artwork retrieved successfully.",
                Data = artwork,
            }
        );
    }

    /// ADD NEW ARTWORK    /// ADD NEW ARTWORK    /// ADD NEW ARTWORK    /// ADD NEW ARTWORK   /// ADD NEW ARTWORK
    [HttpPost("")]
    public async Task<IActionResult> AddArtwork([FromBody] AddArtworkDTO dto)
    {
        var validationResult = await _addArtworkValidator.ValidateAsync(dto);
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
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return NotFound(new ApiResponse<object>
            {
                StatusCode = 404,
                Message = $"Category with ID {dto.CategoryId} does not exist.",
                Data = null
            });
        }

        var artwork = _mapper.Map<Artwork>(dto);
        artwork.CreationTime = DateTime.UtcNow;
        await _context.Artwork.AddAsync(artwork);
        await _context.SaveChangesAsync();
        var response = _mapper.Map<ArtworkResponseDTO>(artwork);
        return Ok(
            new ApiResponse<ArtworkResponseDTO>
            {
                StatusCode = 200,
                Message = "Artwork created successfully.",
                Data = response,
            }
        );
    }
    /// UPDATE ARTWORK    /// UPDATE ARTWORK    /// UPDATE ARTWORK    /// UPDATE ARTWORK    /// UPDATE ARTWORK
    [HttpPut("{id}")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> UpdateArtwork(int id, [FromBody] UpdateArtworkDTO dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                StatusCode = 400,
                Message = "Validation failed.",
                Data = validationResult.Errors.Select(e => new
                {
                    e.PropertyName,
                    e.ErrorMessage
                })
            });
        }

        var artwork = await _context.Artwork.FirstOrDefaultAsync(x => x.Id == id);
        if (artwork == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "Artwork not found.",
                Data = null
            });
        }

        _mapper.Map(dto, artwork);
        await _context.SaveChangesAsync();
        var response = _mapper.Map<UpdateArtworkResponseDTO>(artwork);
        return Ok(new ApiResponse<UpdateArtworkResponseDTO>
        {
            StatusCode = 200,
            Message = "Artwork updated successfully.",
            Data = response
        });
    }
    /// DELETE ARTWORK    /// DELETE ARTWORK    /// DELETE ARTWORK    /// DELETE ARTWORK    /// DELETE ARTWORK
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOrOwner")]
    public async Task<IActionResult> DeleteArtwork(int id)
    {
        var artwork = await _context.Artwork.FindAsync(id);
        if (artwork == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "Artwork not found.",
                Data = null
            });
        }
        _context.Artwork.Remove(artwork);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<Artwork>
        {
            StatusCode = 200,
            Message = "Artwork deleted successfully.",
            Data = artwork
        });
    }
    /// GET ARTWORKS BY CATEGORY    /// GET ARTWORKS BY CATEGORY    /// GET ARTWORKS BY CATEGORY

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetArtworksByCategory(int categoryId)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "Category not found.",
                Data = null
            });
        }

        var artworks = await _context.Artwork
            .Where(a => a.CategoryId == categoryId)
            .ToListAsync();

        return Ok(new ApiResponse<List<Artwork>>
        {
            StatusCode = 200,
            Message = "Artworks retrieved successfully.",
            Data = artworks
        });
    }
}
