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

    // GET ALL USERS
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

}
