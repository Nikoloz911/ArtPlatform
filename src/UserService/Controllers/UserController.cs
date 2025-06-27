using AutoMapper;
using Contracts.Core;
using Contracts.DTO;
using Microsoft.AspNetCore.Http;
using UserService.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;
using Microsoft.AspNetCore.Authorization;
namespace UserService.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public UserController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// GET ALL USERS   /// GET ALL USERS   /// GET ALL USERS   /// GET ALL USERS   /// GET ALL USERS
    [HttpGet("")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        if (users == null || !users.Any())
        {
            var notFoundResponse = new ApiResponse<List<User>>
            {
                StatusCode = 404,
                Message = "No users found",
                Data = null
            };
            return NotFound(notFoundResponse);
        }
        var response = new ApiResponse<List<User>>
        {
            StatusCode = 200,
            Message = "Users retrieved successfully",
            Data = users
        };
        return Ok(response);
    }
    /// GET USER BY ID   /// GET USER BY ID   /// GET USER BY ID   /// GET USER BY ID   /// GET USER BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            var notFoundResponse = new ApiResponse<User>
            {
                StatusCode = 404,
                Message = $"User with ID {id} not found",
                Data = null
            };
            return NotFound(notFoundResponse);
        }

        var response = new ApiResponse<User>
        {
            StatusCode = 200,
            Message = "User retrieved successfully",
            Data = user
        };
        return Ok(response);
    }
    /// UPDATE USER BY ID   /// UPDATE USER BY ID   /// UPDATE USER BY ID   /// UPDATE USER BY ID 
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _mapper.Map(updateDto, user);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Publish the update event
        var publisher = new RabbitMQPublisher();
        publisher.PublishUserUpdated(new UpdateUserEvent
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Password = user.Password,
            Biography = user.Biography,
            ProfilePictureUrl = user.ProfilePictureUrl
        });

        return Ok(user);
    }
    /// DELETE USER BY ID   /// DELETE USER BY ID   /// DELETE USER BY ID   /// DELETE USER BY ID
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new ApiResponse<User>
            {
                StatusCode = 404,
                Message = $"User with ID {id} not found",
                Data = null
            });
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        var publisher = new RabbitMQPublisher();
        publisher.PublishUserDeleted(new UserDeletedEvent { Id = id });

        return Ok(new ApiResponse<User>
        {
            StatusCode = 200,
            Message = "User deleted successfully",
            Data = user
        });
    }
}