using AutoMapper;
using Contracts.Core;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.DTOs;
using FluentValidation.Results;
using PortfolioService.Models;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioService.Controllers;

[Route("api/Portfolios")]
[ApiController]
public class PortfolioController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<AddPortfolioDTO> _validator;
    private readonly IValidator<UpdatePortfolioDTO> _updateValidator;
    public PortfolioController(
      DataContext context,
      IMapper mapper,
      IValidator<AddPortfolioDTO> addValidator,
      IValidator<UpdatePortfolioDTO> updateValidator)
    {
        _context = context;
        _mapper = mapper;
        _validator = addValidator;
        _updateValidator = updateValidator;
    }
    /// GET ALL PORTFOLIOS   /// GET ALL PORTFOLIOS   /// GET ALL PORTFOLIOS   /// GET ALL PORTFOLIOS
    [HttpGet("")]
    public async Task<IActionResult> GetAllPortfolios()
    {
        var portfolios = await _context.Portfolios.ToListAsync();
        if (portfolios == null || !portfolios.Any())
        {
            var notFoundResponse = new ApiResponse<List<Portfolio>>
            {
                StatusCode = 404,
                Message = "No portfolios found",
                Data = null
            };
            return NotFound(notFoundResponse);
        }
        var response = new ApiResponse<List<Portfolio>>
        {
            StatusCode = 200,
            Message = "Portfolios retrieved successfully",
            Data = portfolios
        };
        return Ok(response);
    }
    /// GET PORTFOLIO BY ID   /// GET PORTFOLIO BY ID   /// GET PORTFOLIO BY ID   /// GET PORTFOLIO BY ID  
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPortfolioById(int id)
    {
        var portfolio = await _context.Portfolios.FindAsync(id);
        if (portfolio == null)
        {
            var notFoundResponse = new ApiResponse<Portfolio>
            {
                StatusCode = 404,
                Message = $"Portfolio with ID {id} not found",
                Data = null
            };
            return NotFound(notFoundResponse);
        }
        var response = new ApiResponse<Portfolio>
        {
            StatusCode = 200,
            Message = "Portfolio retrieved successfully",
            Data = portfolio
        };
        return Ok(response);
    }

    /// ADD PORTFOLIO   /// ADD PORTFOLIO   /// ADD PORTFOLIO   /// ADD PORTFOLIO
    [HttpPost("")]
    [Authorize(Policy = "ArtistOnly")]
    public async Task<IActionResult> CreatePortfolio([FromBody] AddPortfolioDTO dto)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

            return BadRequest(new ApiResponse<List<string>>
            {
                StatusCode = 400,
                Message = "Validation failed",
                Data = errorMessages
            });
        }
        var portfolio = _mapper.Map<Portfolio>(dto);
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        var responseDto = _mapper.Map<AddPortfolioResponseDTO>(portfolio);
        var response = new ApiResponse<AddPortfolioResponseDTO>
        {
            StatusCode = 200,
            Message = "Portfolio created successfully",
            Data = responseDto
        };
        return StatusCode(200, response);
    }

    /// UPDATE PORTFOLIO   /// UPDATE PORTFOLIO   /// UPDATE PORTFOLIO   /// UPDATE PORTFOLIO  
    [HttpPut("{id}")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDTO dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiResponse<List<string>>
            {
                StatusCode = 400,
                Message = "Validation failed",
                Data = errors
            });
        }
        var portfolio = await _context.Portfolios.FindAsync(id);
        if (portfolio == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = $"Portfolio with ID {id} not found",
                Data = null
            });
        }
        _mapper.Map(dto, portfolio); 
        await _context.SaveChangesAsync();
        var responseDto = _mapper.Map<UpdatePortfolioResponseDTO>(portfolio);
        return Ok(new ApiResponse<UpdatePortfolioResponseDTO>
        {
            StatusCode = 200,
            Message = "Portfolio updated successfully",
            Data = responseDto
        });
    }
    /// DELETE PORTFOLIO   /// DELETE PORTFOLIO   /// DELETE PORTFOLIO   /// DELETE PORTFOLIO
    [HttpDelete("{id}")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
        var portfolio = await _context.Portfolios.FindAsync(id);

        if (portfolio == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = $"Portfolio with ID {id} not found",
                Data = null
            });
        }
        _context.Portfolios.Remove(portfolio);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<string>
        {
            StatusCode = 200,
            Message = "Portfolio deleted successfully",
            Data = null
        });
    }
}
