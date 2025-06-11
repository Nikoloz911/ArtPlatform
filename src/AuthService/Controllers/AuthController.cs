using AuthService.Data;
using AuthService.Models;
using AuthService.DTOs;   
using Contracts.Core;
using Contracts.DTO;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;

    public AuthController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDto)
    {
        // Map DTO to User entity
        var user = new User
        {
            Name = registerUserDto.Name,
            Email = registerUserDto.Email,
            Password = registerUserDto.Password,
            Biography = registerUserDto.Biography,
            ProfilePictureUrl = registerUserDto.ProfilePictureUrl
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

    
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "user_created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var userCreatedEvent = new UserCreatedEvent
        {
            Name = user.Name,
            Email = user.Email,
            Password = user.Password,
            Biography = user.Biography,
            ProfilePictureUrl = user.ProfilePictureUrl
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userCreatedEvent));

        channel.BasicPublish(
            exchange: "",
            routingKey: "user_created",
            basicProperties: null,
            body: body
        );

        return Ok(new ApiResponse<User>
        {
            StatusCode = 200,
            Message = "User registered and event sent",
            Data = user
        });
    }
}
