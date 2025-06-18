using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AutoMapper;
using CommonUtils.SMTP;
using Contracts.Core;
using Contracts.DTO;
using Contracts.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using FluentValidation.Results;
namespace AuthService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterUserDTO> _validator;

    public AuthController(DataContext context, IMapper mapper, IValidator<RegisterUserDTO> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDto)
    {
        /// VALIDATE THE REQUEST
        ValidationResult validationResult = await _validator.ValidateAsync(registerUserDto);
        if (!validationResult.IsValid)
        {
            // Return all validation errors as a list
            return BadRequest(new ApiResponse<List<string>>
            {
                StatusCode = 400,
                Message = "Validation Failed",
                Data = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }
        // Convert role string to enum
        if (!Enum.TryParse<USER_ROLES>(registerUserDto.Role, true, out var parsedRole))
        {
            return BadRequest(new ApiResponse<string>
            {
                StatusCode = 400,
                Message = "Invalid role. Must be 'Artist', 'Critic', or 'Admin'.",
                Data = null
            });
        }

        // Generate verification code
        string verificationCode = SMTP_Registration.GenerateVerificationCode();

        // Map DTO to User entity
        var user = _mapper.Map<User>(registerUserDto);
        user.Role = parsedRole;
        user.IsVerified = false;
        user.VerificationCode = verificationCode;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send verification email
        try
        {
            SMTP_Registration.EmailSender(user.Email, user.Name, verificationCode);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>
            {
                StatusCode = 500,
                Message = "User created but failed to send verification email.",
                Data = ex.Message
            });
        }

        // Send event to RabbitMQ
        var userCreatedEvent = _mapper.Map<UserCreatedEvent>(user);

        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "user_created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

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
            Message = "User registered, verification email sent, and event published.",
            Data = user
        });
    }
}