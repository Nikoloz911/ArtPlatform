using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using Contracts.Core;
using UserService.Models;
namespace UserService.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataContext _context;
    public UserController(DataContext context)
    {
        _context = context;
    }

    /// GET ALL USERS   /// GET ALL USERS   /// GET ALL USERS   /// GET ALL USERS   /// GET ALL USERS
    [HttpGet("")]
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

    //    PUT /api/users/{id
    //} - მომხმარებლის ინფორმაციის განახლება


}
