using AutoMapper;
using Contracts.Core;
using Contracts.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.Data;
using SubscriptionService.DTOs;
using SubscriptionService.Models;

namespace SubscriptionService.Controllers;

[Route("api/subscription")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public SubscriptionController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// ADD SUBSCRIPTION TO ARTIST    /// ADD SUBSCRIPTION TO ARTIST    /// ADD SUBSCRIPTION TO ARTIST
    [HttpPost("{artistId}")]
    public async Task<ActionResult<ApiResponse<SubscriptionDTO>>> SubscribeToArtist(int artistId)
    {
        var artist = await _context.Users.FirstOrDefaultAsync(u => u.Id == artistId);

        if (artist == null)
        {
            return NotFound(
                new ApiResponse<SubscriptionDTO>
                {
                    StatusCode = 404,
                    Message = "Artist not found",
                    Data = null,
                }
            );
        }

        if (artist.Role != USER_ROLES.ARTIST)
        {
            return BadRequest(
                new ApiResponse<SubscriptionDTO>
                {
                    StatusCode = 400,
                    Message = "User is not an artist",
                    Data = null,
                }
            );
        }

        var subscription = new Subscription { ArtistId = artistId, CreationDate = DateTime.UtcNow };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        var subscriptionDto = _mapper.Map<SubscriptionDTO>(subscription);
        return Ok(
            new ApiResponse<SubscriptionDTO>
            {
                StatusCode = 200,
                Message = "Subscribed successfully",
                Data = subscriptionDto,
            }
        );
    }

    /// GET ALL SUBSCRIPTIONS OF USER /// GET ALL SUBSCRIPTIONS OF USER /// GET ALL SUBSCRIPTIONS OF USER
    [HttpGet("followers/{artistId}")]
    public async Task<ActionResult<ApiResponse<List<FollowerDTO>>>> GetFollowers(int artistId)
    {
        var artist = await _context.Users.FirstOrDefaultAsync(u => u.Id == artistId);

        if (artist == null)
        {
            return NotFound(
                new ApiResponse<List<FollowerDTO>>
                {
                    StatusCode = 404,
                    Message = "Artist not found",
                    Data = null,
                }
            );
        }

        if (artist.Role != USER_ROLES.ARTIST)
        {
            return BadRequest(
                new ApiResponse<List<FollowerDTO>>
                {
                    StatusCode = 400,
                    Message = "User is not an artist",
                    Data = null,
                }
            );
        }

        var subscriptions = await _context
            .Subscriptions.Where(s => s.ArtistId == artistId)
            .ToListAsync();

        var followerDTOs = _mapper.Map<List<FollowerDTO>>(subscriptions);

        return Ok(
            new ApiResponse<List<FollowerDTO>>
            {
                StatusCode = 200,
                Message = "Followers retrieved successfully",
                Data = followerDTOs,
            }
        );
    }

    /// DELETE SUBSCRIPTION FROM ARTIST /// DELETE SUBSCRIPTION FROM ARTIST /// DELETE SUBSCRIPTION FROM ARTIST
    [HttpDelete("{artistId}")]
    public async Task<ActionResult<ApiResponse<string>>> UnsubscribeFromArtist(int artistId)
    {
        var artist = await _context.Users.FirstOrDefaultAsync(u => u.Id == artistId);
        if (artist == null)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = "Artist not found",
                    Data = null,
                }
            );
        }

        if (artist.Role != USER_ROLES.ARTIST)
        {
            return BadRequest(
                new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = "User is not an artist",
                    Data = null,
                }
            );
        }

        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s =>
            s.ArtistId == artistId
        );

        if (subscription == null)
        {
            return NotFound(
                new ApiResponse<string>
                {
                    StatusCode = 404,
                    Message = "Subscription not found",
                    Data = null,
                }
            );
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        return Ok(
            new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Unsubscribed successfully",
                Data = null,
            }
        );
    }
}
