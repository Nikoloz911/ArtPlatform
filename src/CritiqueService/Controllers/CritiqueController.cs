using AutoMapper;
using Contracts.Core;
using CritiqueService.Data;
using CritiqueService.DTOs;
using CritiqueService.Models;
using CritiqueService.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CritiqueService.Controllers;
[Route("api/critique")]
[ApiController]
public class CritiqueController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<AddCritiqueDTO> _validator;
    public CritiqueController(DataContext context, IMapper mapper, IValidator<AddCritiqueDTO> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }
    /// GET CRITIQUES BY ARTWORK ID   /// GET CRITIQUES BY ARTWORK ID
    [HttpGet("artwork/{artworkId}")]
    public async Task<ActionResult<ApiResponse<List<Critique>>>> GetCritiquesByArtwork(int artworkId)
    {
        var critiques = await _context.Critiques
                            .Where(c => c.ArtworkId == artworkId)
                            .ToListAsync();

        if (critiques == null || critiques.Count == 0)
        {
            return NotFound(new ApiResponse<List<Critique>>
            {
                StatusCode = 404,
                Message = "No critiques found for the specified artwork",
                Data = null
            });
        }

        var response = new ApiResponse<List<Critique>>
        {
            StatusCode = 200,
            Message = "Critiques retrieved successfully",
            Data = critiques
        };
        return Ok(response);
    }
    /// ADD CRITIQUE    /// ADD CRITIQUE   /// ADD CRITIQUE   /// ADD CRITIQUE  

    [HttpPost("")]
    [Authorize (Policy = "CriticOnly")]
    public async Task<ActionResult<ApiResponse<AddCritiqueResponseDTO>>> AddCritique(AddCritiqueDTO dto)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiResponse<AddCritiqueDTO>
            {
                StatusCode = 400,
                Message = "Validation failed",
                Data = null
            });
        }

        bool artworkExists = await _context.Artworks.AnyAsync(a => a.Id == dto.ArtworkId);
        if (!artworkExists)
        {
            return NotFound(new ApiResponse<AddCritiqueResponseDTO>
            {
                StatusCode = 404,
                Message = $"Artwork with ID {dto.ArtworkId} not found.",
                Data = null
            });
        }

        var critique = _mapper.Map<Critique>(dto);
        critique.CreationDate = DateTime.UtcNow; 
        await _context.Critiques.AddAsync(critique);
        await _context.SaveChangesAsync();
        var responseDto = _mapper.Map<AddCritiqueResponseDTO>(critique);
        return CreatedAtAction(nameof(AddCritique), new { id = critique.Id }, new ApiResponse<AddCritiqueResponseDTO>
        {
            StatusCode = 200,
            Message = "Critique added successfully",
            Data = responseDto
        });
    }
    /// UPDATE CRITIQUE   /// UPDATE CRITIQUE   /// UPDATE CRITIQUE   /// UPDATE CRITIQUE
    [HttpPut("{id}")]
    [Authorize (Policy = "OwnerOnly")]
    public async Task<ActionResult<ApiResponse<UpdateCritiqueResponseDTO>>> UpdateCritique(int id, UpdateCritiqueDTO dto)
    {
        var validator = new UpdateCritiqueValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<UpdateCritiqueDTO>
            {
                StatusCode = 400,
                Message = "Validation failed",
                Data = null
            });
        }

        var critique = await _context.Critiques.FindAsync(id);
        if (critique == null)
        {
            return NotFound(new ApiResponse<UpdateCritiqueResponseDTO>
            {
                StatusCode = 404,
                Message = $"Critique with ID {id} not found.",
                Data = null
            });
        }

        critique.Rating = dto.Rating;
        critique.Text = dto.Text;
        await _context.SaveChangesAsync();
        var responseDto = _mapper.Map<UpdateCritiqueResponseDTO>(critique);
        return Ok(new ApiResponse<UpdateCritiqueResponseDTO>
        {
            StatusCode = 200,
            Message = "Critique updated successfully",
            Data = responseDto
        });
    }
    /// DELETE CRITIQUE   /// DELETE CRITIQUE   /// DELETE CRITIQUE   /// DELETE CRITIQUE
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCritique(int id)
    {
        var critique = await _context.Critiques.FindAsync(id);
        if (critique == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = $"Critique with ID {id} not found.",
                Data = null
            });
        }

        _context.Critiques.Remove(critique);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<string>
        {
            StatusCode = 200,
            Message = "Critique deleted successfully",
            Data = null
        });
    }
}
