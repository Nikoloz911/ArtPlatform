using ArtworkService.Data;
using ArtworkService.DTOs;
using ArtworkService.Models;
using AutoMapper;
using Contracts.Core;
using FluentValidation;
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
    public ArtworkController(DataContext context, IMapper mapper, IValidator<AddArtworkDTO> addArtworkValidator)
    {
        _context = context;
        _mapper = mapper;
        _addArtworkValidator = addArtworkValidator;
    }
    /// ADD NEW ARTWORK    /// ADD NEW ARTWORK    /// ADD NEW ARTWORK    /// ADD NEW ARTWORK   /// ADD NEW ARTWORK
    [HttpPost("")]
    public async Task<IActionResult> AddArtwork([FromBody] AddArtworkDTO dto)
    {
        var validationResult = await _addArtworkValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                StatusCode = 400,
                Message = "Validation failed.",
                Data = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }

        var artwork = _mapper.Map<Artwork>(dto);
        artwork.CreationTime = DateTime.UtcNow;
        await _context.Artwork.AddAsync(artwork);
        await _context.SaveChangesAsync();
        var response = _mapper.Map<ArtworkResponseDTO>(artwork);
        return Ok(new ApiResponse<ArtworkResponseDTO>
        {
            StatusCode = 200,
            Message = "Artwork created successfully.",
            Data = response
        });
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
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(a => a.Category == filter.Category);
        }
        if (filter.CreationTime.HasValue)
        {
            var creationDate = filter.CreationTime.Value.Date;
            query = query.Where(a => a.CreationTime.Date == creationDate);
        }
        var artworks = await query
            .OrderByDescending(a => a.CreationTime)
            .ToListAsync();

        if (artworks.Count == 0)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "No artworks found matching the criteria.",
                Data = null
            });
        }
        var response = _mapper.Map<List<ArtworkResponseDTO>>(artworks);
        return Ok(new ApiResponse<List<ArtworkResponseDTO>>
        {
            StatusCode = 200,
            Message = "Filtered artwork list retrieved.",
            Data = response
        });
    }
    /// GET ARTWORK BY ID    /// GET ARTWORK BY ID    /// GET ARTWORK BY ID    /// GET ARTWORK BY ID  
    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtworkById(int id)
    {
        var artwork = await _context.Artwork.FindAsync(id);
        if (artwork == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = $"Artwork with ID {id} not found.",
                Data = null
            });
        }
        return Ok(new ApiResponse<Artwork>
        {
            StatusCode = 200,
            Message = "Artwork retrieved successfully.",
            Data = artwork
        });
    }

    //PUT /api/artworks/{id} -ნამუშევრის განახლება(Owner)
    //DELETE / api / artworks /{ id}
    //-ნამუშევრის წაშლა(Owner / Admin)
    //GET / api / artworks / category /{ categoryId}
    //-ნამუშევრები კატეგორიის მიხედვით

}
